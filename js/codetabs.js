$( document ).ready(function() {
    var allTabsInPage = $('[id^="thetabs"]');
    activateAllTabs(allTabsInPage);
});

function activateAllTabs(allTabsInPage) { 
    $.each(allTabsInPage, function(i, val){
        var localStorageName = "botFrameworkDocsActiveTab";
        $( "#"+ val.id ).tabs({
            active: localStorage[localStorageName] ? activeTabIndex(allTabsInPage[i], localStorage[localStorageName]) : 0,
            activate: function(event, ui) {
                localStorage.setItem(localStorageName, ui.newTab[0].dataset.lang);//
                activateAllTabs(allTabsInPage);
            }
        });
    });
}

function activeTabIndex(tab, lang) {
    var activeTab = $("#"+tab.id).children().find("[data-lang='" + lang + "']");
    if (activeTab.length > 0) {
        var index = activeTab.parent().children().index(activeTab);
        return index;
    } 
    return 0;
}