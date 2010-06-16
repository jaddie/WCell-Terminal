$(function() {
	$(document).bind('contextmenu', function(e) {
		return false;
	});
	$('#wrapper').dialog({
		title: 'WCell.Terminal v1.0',
		width: 700,
		height: 500,
		resize: function(event, ui) {
			wcell.resize();
		}
	});
	$('#tabs').tabs();
	$('.ui-dialog').center();
	var domain = document.domain;
	var fullpath = top.location.href.split(domain);
	fullpath = fullpath[1];
	path = fullpath.lastIndexOf('/');
	path = fullpath.substring(0, path + 1);
	var wcell = new WCell('http://' + domain + path + 'connector/');
});
