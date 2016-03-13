var currentpath = window.location.pathname;
$( document ).ready(function() {
    $('.page-link[href="' + currentpath + '"]').addClass('navselected');
	if( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
		var urlhash = document.location.hash;
		if (urlhash == '#navtitle') {
			$('html, body').animate({
				scrollTop: $("h1").offset().top
			}, 1000);
		}
	}
});
