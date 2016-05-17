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
    // open left nav container if a page is currently selected
    var currentNav = $(".page-link.navselected").closest(".navContainer").prev();
    if (currentNav.length == 0 && !isNodeRefDoc()) {
        // show all nodes
        $( ".level1.parent" ).show();
    } else {
        // hide top level links, show back to top
        $(".level0").hide();
        $(".backToHome").show();
        if (isNodeRefDoc()) {
            var currentListHref = $('a[href*="/builder/node/sdkreference/"]').first();
            currentListHref.addClass("navselected");
            currentNav = currentListHref.closest(".navContainer").prev();
        } 
        toggleNav(currentNav, 0);
    }
    
    // left nav toggle on top level container
    $( ".level1.parent" ).click(function() {
        toggleNav($(this), 400);
    });
});

function toggleNav(parent, dur) {    
    $content = parent.next();
    $content.slideToggle(dur);
    parent.show();
    parent.toggleClass("rotate");
}

// 
function isNodeRefDoc() {
    var currentUrl = window.location.href;
    if (currentUrl.indexOf("/sdkreference/nodejs/") != -1) {
        return true;
    } 
    return false;
}