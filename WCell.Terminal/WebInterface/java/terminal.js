$(function() {
	$('#console').dialog({
		title: 'WCell.Terminal v1.0',
		minWidth: 1000,
		minHeight: 600,
		width: 1000,
		height: 600,
		resizeStart: function(event, ui) {
			if ($.browser.msie) {
				$('#console').css('width', '100%');
			}				
		}, 
		resize: function(event, ui) {
			if ($.browser.msie) {
				$('#console').css('width', '100%');
				$('#tab-1').css('height', $('#console').height() - 45 + 'px');
			}
		},
		resizeStop: function(event, ui) {
			if ($.browser.msie) {
				$('#console').css('width', '100%');
			}			
		}
	});
	$('#tabs').tabs();
});
