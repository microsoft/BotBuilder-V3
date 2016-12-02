/*! Bing Search Helper v1.0.0 - requires jQuery v1.7.2 */
function getParameterByName(name, url) {
    if (!url) url = window.location.href;
    name = name.replace(/[\[\]]/g, "\\$&");
    var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
        results = regex.exec(url);
    if (!results) return null;
    if (!results[2]) return '';
    return decodeURIComponent(results[2].replace(/\+/g, " "));
}

// this function is used in the C# docs too
function setProgrammingLanguage() {
    var storedLang = localStorage.botFrameworkDocsSearchLang ? localStorage.botFrameworkDocsSearchLang : '';
    $('#lang-select option[value="'+ storedLang +'"]').prop('selected', true);
}

$( document ).ready(function() {
    // $('#lang-select').remove();
    // $('#q').css('padding','3px 25px 3px 10px');
    
    var q = getParameterByName('q');
    var mkt = getParameterByName('mkt');
    var lang = getParameterByName('lang');
    var v = getParameterByName('v');
	var data = { q: q, mkt: mkt, v: v, lang: lang };

    setProgrammingLanguage();
    
    if (q) {
        $('#q').val(q);
        search(data);
    }

    // Attaches a click handler to the button.
    $('#bt_search').click(function (e) {
        q = $('#q').val();
        if (q) {
            lang = $('#lang-select').find('option:selected').val() ? $('#lang-select').find('option:selected').val() : '';
            localStorage.setItem("botFrameworkDocsSearchLang", lang);
            var formaction = $('#docs-search-form').attr('action');

            if (!formaction) {
                e.preventDefault();
                // Clear the results div.
                $('#search-results').empty();
                mkt = $('#mkt').val() ? $('#mkt').val() : '';
                v = $('#v').val() ? $('#v').val() : '';
                data = { q: q, mkt: mkt, v: v, lang: lang };
                updateAddressBar(data);
                search(data);
            } 
        } else {
            e.preventDefault();
        }
    });

    // Performs the search.
    function search(data) {
        // Set the page title
        var query = data["q"].substring(0,Math.min(99,data["q"].length));
        var displayQuery = document.createElement('span');
        $(displayQuery).addClass('displayQuery').text(query);
        $('.post-title').text('Search results for: ');
        $('.post-title').append(displayQuery);
        $('#search-progress').addClass("loading");
        // Establish the data to pass to the proxy.
        var host = 'https://bots.botframework.com/api/docssearch';
        // Calls the proxy, passing the query, service operation and market.
        $.ajax({
            url: host,
            type: 'GET',
            dataType: 'json',
            data: data,
            success: function(obj) {
                if (obj.webPages !== undefined) {
                    var items = obj.webPages.value;
                    if (items.length > 0) {
                        for (var k = 0, len = items.length; k < len; k++) {
                            var item = items[k];
                            showWebResult(item);
                        }
                    } 
                } else {
                    $('#search-results').html('no results');
                }
            },
            error: function(err) {
               $('#search-results').html('no results');
            },
            complete: function() {
                $('#search-progress').removeClass("loading");
            }
        });
    }

    // Shows one item of Web result.
    function showWebResult(item) {
        var container = document.createElement('div');
        $(container).addClass('search-result-item');
        var p = document.createElement('p');
        var a = document.createElement('a');
        var pp = document.createElement('p');
        a.href = item.url;
        $(a).append(item.name);
        $(p).append(item.snippet);
        $(pp).append(item.displayUrl);
        $(pp).addClass('bingSearchUrl');
        $(container).append(a, pp, p);
        $('#search-results').append(container);
    }

    function updateAddressBar(data) {
        window.history.pushState("", "", "?q=" + data["q"] + "&mkt=" + data["mkt"] + "&v=" + data["v"] + "&lang=" + data["lang"]);
    }
});
