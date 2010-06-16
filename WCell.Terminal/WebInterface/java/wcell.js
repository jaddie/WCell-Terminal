// IE does not define XMLHttpRequest by default, so we provide a suitable
// wrapper.
if (typeof XMLHttpRequest == 'undefined') {
  XMLHttpRequest = function() {
    try { return new ActiveXObject('Msxml2.XMLHTTP.6.0');} catch (e) { }
    try { return new ActiveXObject('Msxml2.XMLHTTP.3.0');} catch (e) { }
    try { return new ActiveXObject('Msxml2.XMLHTTP');    } catch (e) { }
    try { return new ActiveXObject('Microsoft.XMLHTTP'); } catch (e) { }
    throw new Error('');
  };
}

function extend(subClass, baseClass) {
  function inheritance() { }
  inheritance.prototype          = baseClass.prototype;
  subClass.prototype             = new inheritance();
  subClass.prototype.constructor = subClass;
  subClass.prototype.superClass  = baseClass.prototype;
};

function WCell(url, container) {
  if (url == undefined) {
    this.rooturl    = document.location.href;
    this.url        = document.location.href.replace(/[?#].*/, '');
  } else {
    this.rooturl    = url;
    this.url        = url;
  }
  if (document.location.hash != '') {
    var hash        = decodeURIComponent(document.location.hash).
                      replace(/^#/, '');
    this.nextUrl    = hash.replace(/,.*/, '');
    this.session    = hash.replace(/[^,]*,/, '');
  } else {
    this.nextUrl    = this.url;
    this.session    = null;
  }
  this.pendingKeys  = '';
  this.keysInFlight = false;
  this.connected    = false;
  this.superClass.constructor.call(this, container);

  // We have to initiate the first XMLHttpRequest from a timer. Otherwise,
  // Chrome never realizes that the page has loaded.
  setTimeout(function(obj) {
               return function() {
                 obj.sendRequest();
               };
             }(this), 1);
};
extend(WCell, VT100);

WCell.prototype.resize = function() {
  this.hideContextMenu();
  this.resizer();
  this.showCurrentSize();
}

WCell.prototype.sessionClosed = function() {
  try {
    this.connected    = false;
    if (this.session) {
      this.session    = undefined;
      if (this.cursorX > 0) {
        this.vt100('\r\n');
      }
      this.vt100('Session closed.');
    }
    this.showReconnect(true);
  } catch (e) {
  }
};

WCell.prototype.reconnect = function() {
  this.showReconnect(false);
  if (!this.session) {
    if (document.location.hash != '') {
      // A shellinaboxd daemon launched from a CGI only allows a single
      // session. In order to reconnect, we must reload the frame definition
      // and obtain a new port number. As this is a different origin, we
      // need to get enclosing page to help us.
      parent.location        = this.nextUrl;
    } else {
      if (this.url != this.nextUrl) {
        document.location.replace(this.nextUrl);
      } else {
        this.pendingKeys     = '';
        this.keysInFlight    = false;
        this.reset(true);
        this.sendRequest();
      }
    }
  }
  return false;
};

WCell.prototype.sendRequest = function(request) {
  if (request == undefined) {
    request                  = new XMLHttpRequest();
  }
  request.open('POST', this.url + '?', true);
  request.setRequestHeader('Cache-Control', 'no-cache');
  request.setRequestHeader('Content-Type',
                           'application/x-www-form-urlencoded; charset=utf-8');
  var content                = 'width=' + this.terminalWidth +
                               '&height=' + this.terminalHeight +
                               (this.session ? '&session=' +
                                encodeURIComponent(this.session) : '&rooturl='+
                                encodeURIComponent(this.rooturl));
  request.setRequestHeader('Content-Length', content.length);

  request.onreadystatechange = function(obj) {
    return function() {
             try {
               return obj.onReadyStateChange(request);
             } catch (e) {
               obj.sessionClosed();
             }
           }
    }(this);
  request.send(content);
};

WCell.prototype.onReadyStateChange = function(request) {
  if (request.readyState == 4 /* XHR_LOADED */) {
    if (request.status == 200) {
      this.connected = true;
      var response   = eval('(' + request.responseText + ')');
      if (response.data) {
        this.vt100(response.data);
      }

      if (!response.session ||
          this.session && this.session != response.session) {
        this.sessionClosed();
      } else {
        this.session = response.session;
        this.sendRequest(request);
      }
    } else if (request.status == 0) {
      // Time Out
      this.sendRequest(request);
    } else {
      this.sessionClosed();
    }
  }
};

WCell.prototype.sendKeys = function(keys) {
  if (!this.connected) {
    return;
  }
  if (this.keysInFlight || this.session == undefined) {
    this.pendingKeys          += keys;
  } else {
    this.keysInFlight          = true;
    keys                       = this.pendingKeys + keys;
    this.pendingKeys           = '';
    var request                = new XMLHttpRequest();
    request.open('POST', this.url + '?', true);
    request.setRequestHeader('Cache-Control', 'no-cache');
    request.setRequestHeader('Content-Type',
                           'application/x-www-form-urlencoded; charset=utf-8');
    var content                = 'width=' + this.terminalWidth +
                                 '&height=' + this.terminalHeight +
                                 '&session=' +encodeURIComponent(this.session)+
                                 '&keys=' + encodeURIComponent(keys);
    request.setRequestHeader('Content-Length', content.length);
    request.onreadystatechange = function(obj) {
      return function() {
               try {
                 return obj.keyPressReadyStateChange(request);
               } catch (e) {
               }
             }
      }(this);
    request.send(content);
  }
};

WCell.prototype.keyPressReadyStateChange = function(request) {
  if (request.readyState == 4 /* XHR_LOADED */) {
    this.keysInFlight = false;
    if (this.pendingKeys) {
      this.sendKeys('');
    }
  }
};

WCell.prototype.keysPressed = function(ch) {
  var hex = '0123456789ABCDEF';
  var s   = '';
  for (var i = 0; i < ch.length; i++) {
    var c = ch.charCodeAt(i);
    if (c < 128) {
      s += hex.charAt(c >> 4) + hex.charAt(c & 0xF);
    } else if (c < 0x800) {
      s += hex.charAt(0xC +  (c >> 10)       ) +
           hex.charAt(       (c >>  6) & 0xF ) +
           hex.charAt(0x8 + ((c >>  4) & 0x3)) +
           hex.charAt(        c        & 0xF );
    } else if (c < 0x10000) {
      s += 'E'                                 +
           hex.charAt(       (c >> 12)       ) +
           hex.charAt(0x8 +  (c >> 10) & 0x3 ) +
           hex.charAt(       (c >>  6) & 0xF ) +
           hex.charAt(0x8 + ((c >>  4) & 0x3)) +
           hex.charAt(        c        & 0xF );
    } else if (c < 0x110000) {
      s += 'F'                                 +
           hex.charAt(       (c >> 18)       ) +
           hex.charAt(0x8 +  (c >> 16) & 0x3 ) +
           hex.charAt(       (c >> 12) & 0xF ) +
           hex.charAt(0x8 +  (c >> 10) & 0x3 ) +
           hex.charAt(       (c >>  6) & 0xF ) +
           hex.charAt(0x8 + ((c >>  4) & 0x3)) +
           hex.charAt(        c        & 0xF );
    }
  }
  this.sendKeys(s);
};

WCell.prototype.resized = function(w, h) {
  // Do not send a resize request until we are fully initialized.
  if (this.session) {
    // sendKeys() always transmits the current terminal size. So, flush all
    // pending keys.
    this.sendKeys('');
  }
};

WCell.prototype.toggleSSL = function() {
  if (document.location.hash != '') {
    if (this.nextUrl.match(/\?plain$/)) {
      this.nextUrl    = this.nextUrl.replace(/\?plain$/, '');
    } else {
      this.nextUrl    = this.nextUrl.replace(/[?#].*/, '') + '?plain';
    }
    if (!this.session) {
      parent.location = this.nextUrl;
    }
  } else {
    this.nextUrl      = this.nextUrl.match(/^https:/)
           ? this.nextUrl.replace(/^https:/, 'http:').replace(/\/*$/, '/plain')
           : this.nextUrl.replace(/^http/, 'https').replace(/\/*plain$/, '');
  }
  if (this.nextUrl.match(/^[:]*:\/\/[^/]*$/)) {
    this.nextUrl     += '/';
  }
  if (this.session && this.nextUrl != this.url) {
    alert('This change will take effect the next time you login.');
  }
};

WCell.prototype.extendContextMenu = function(entries, actions) {
  // Modify the entries and actions in place, adding any locally defined
  // menu entries.
  var oldActions            = [ ];
  for (var i = 0; i < actions.length; i++) {
    oldActions[i]           = actions[i];
  }
  for (var node = entries.firstChild, i = 0, j = 0; node;
       node = node.nextSibling) {
    if (node.tagName == 'LI') {
      actions[i++]          = oldActions[j++];
      if (node.id == "endconfig") {
        node.id             = '';
        if (typeof serverSupportsSSL != 'undefined' && serverSupportsSSL &&
            !(typeof disableSSLMenu != 'undefined' && disableSSLMenu)) {
          // If the server supports both SSL and plain text connections,
          // provide a menu entry to switch between the two.
          var newNode       = document.createElement('li');
          var isSecure;
          if (document.location.hash != '') {
            isSecure        = !this.nextUrl.match(/\?plain$/);
          } else {
            isSecure        =  this.nextUrl.match(/^https:/);
          }
          newNode.innerHTML = (isSecure ? '&#10004; ' : '') + 'Secure';
          if (node.nextSibling) {
            entries.insertBefore(newNode, node.nextSibling);
          } else {
            entries.appendChild(newNode);
          }
          actions[i++]      = this.toggleSSL;
          node              = newNode;
        }
        node.id             = 'endconfig';
      }
    }
  }
  
};

WCell.prototype.about = function() {
  alert("Shell In A Box version " + "2.10 (revision 204)" +
        "\nCopyright 2008-2010 by Markus Gutschke\n" +
        "For more information check http://shellinabox.com" +
        (typeof serverSupportsSSL != 'undefined' && serverSupportsSSL ?
         "\n\n" +
         "This product includes software developed by the OpenSSL Project\n" +
         "for use in the OpenSSL Toolkit. (http://www.openssl.org/)\n" +
         "\n" +
         "This product includes cryptographic software written by " +
         "Eric Young\n(eay@cryptsoft.com)" :
         ""));
};
