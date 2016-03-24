$( document ).ready(function() {
	// If on mobile, scroll page to first header
	if( /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent) ) {
		var linkfromnav = '#navtitle';
		var urlhash = document.location.hash;
		if (urlhash == linkfromnav) {
			$('html, body').animate({
				scrollTop: $("h1").offset().top
			}, 1500);
		}
	}
    /*
    $( ".accordion" ).accordion({
        collapsible: true,
        heightStyle: "content"
    });
    */
});
