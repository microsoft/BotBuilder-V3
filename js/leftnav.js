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
    var currentNav = getClosestNavcontainer($(".page-link.navselected"));
    var isNodeRefDocVar = isNodeRefDoc();
    if (currentNav.length == 0 && isNodeRefDocVar == "") {
        // show all nodes
        $( ".level1.parent" ).show();
    } else {
        // hide top level links, show back to top
        $(".level0").hide();
        $(".backToHome").show();
        if (isNodeRefDocVar == "chat") {
            var currentListHref = $('a[href*="/node/builder/chat-ref/"]').first();
            currentListHref.addClass("navselected");
            currentNav = getClosestNavcontainer(currentListHref);
        } 
        if (isNodeRefDocVar == "calling") {
            var currentListHref = $('a[href*="/node/builder/calling-ref/"]').first();
            currentListHref.addClass("navselected");
            currentNav = getClosestNavcontainer(currentListHref);
        } 
        toggleNav(currentNav, 0);
    }
    
    // left nav toggle on top level container
    $( ".level1.parent" ).click(function() {
        toggleNav($(this), 400);
    });

    //$(".brand-primary").after('<div class="home-intro"><div class="upgrade-message"><span>There\'s a new version of the Microsoft Bot Framework. Update your bot now to use cards, carousels and action buttons. </span><a href="https://aka.ms/bf-migrate"><span>Learn how</span></a></div></div>');
    
});

function getClosestNavcontainer(currentElement) {
    return currentElement.closest(".navContainer").prev();
}


function toggleNav(parent, dur) {    
    //$content = parent.children().first();
    $content = parent.next();
    $content.slideToggle(dur);
    parent.show();
    parent.toggleClass("rotate");
}

// 
function isNodeRefDoc() {
    var currentUrl = window.location.href;
    if (currentUrl.indexOf("/node/builder/chat-reference/") != -1) {
        return "chat";
    } 
    if (currentUrl.indexOf("/node/builder/calling-reference/") != -1) {
        return "calling";
    } 
    return "";
}
