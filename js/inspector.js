function setFocused(text){
	localStorage.setItem("focused", text);
	localStorage.setItem("scrollTop", document.body.scrollTop.toString());
}

function openDD(channel, feature, example, type){
	if(type == "channel"){
		document.getElementById("channel_" + channel + "_feature_" + feature + "_example_" + example + "_channels_ul").style.display = "block";	
	}
	else{
		document.getElementById("channel_" + channel + "_feature_" + feature + "_example_" + example + "_features_ul").style.display = "block";		
	}
}

//URL Methods
function changeURL(channel, feature, example){
		localStorage.setItem("last_feature", feature);
		var url_location = window.location.origin + "/en-us/channel-inspector/channels/" + channel + "?f=" + feature + "&e=" + example;
		window.location = url_location;
}

function channelChanged(select){
    localStorage.setItem("focused", "channels");
    changeURL(select.value, select.parentElement.elements["features"].value, "example1");
}

function featureChanged(select){
    localStorage.setItem("focused", "features");        
    changeURL(select.parentElement.elements["channels"].value, select.value, "example1");
}

function exampleChanged(example_name){
    localStorage.setItem("focused", "examples");
    changeURL(document.getElementById('channels').value, document.getElementById('features').value, example_name);
}

//URL Parameter Methods
function queryStringParams(querystring) {
    if (querystring === void 0) { querystring = document.location.search.substring(1); }
    if (querystring) {
        var pairs = querystring.split('&');
        for (var i = 0; i < pairs.length; i++) {
            var pair = pairs[i].split('=');
            this[pair[0]] = decodeURIComponent(pair[1]);
        }
    }        
}

//Method to copy text to memory from any element 
function copyToClipboard(element) {
	var $temp = $("<input>");
	$("body").append($temp);
	$temp.val($(element).text()).select();
	document.execCommand("copy");
	$temp.remove();
}

//Defining page behavior using jQuery
function inspector(){
    var qp = new queryStringParams(window.location.search.substring(1));        

	//Global variables
	var channel = $('#channel_name').text();	
    var feature =  qp["f"];
    var example = qp["e"];

    //Validating Global Variables
    if(feature === undefined || feature === null || feature == ""){
        //Getting last_feature from localStorage 
		if(localStorage.getItem("last_feature") === null || localStorage.getItem("last_feature") === "" || 
		   localStorage.getItem("last_feature") === "ChannelData" || localStorage.getItem("last_feature") === "Keyboards"){
            feature = "Buttons";
		}
        else{
            feature = localStorage.getItem("last_feature");
        }
        example = "example1";
    }

	//Channel-Feature variables
	var channel_feature = "#channel_" + channel + "_feature_" + feature;
	var examples = channel_feature + "_examples";
	var current_example = channel_feature + "_current_example";
	var examples_num = channel_feature + "_examples_num";
	
	//Channel-Feature-Example variables
	var channel_feature_example = channel_feature + "_example_" + example;
	var channel_feature_no_example = channel_feature + "_example_example";
	var inspector_container = channel_feature_example + "_container";
	var inspector_image_url = channel_feature_example + "_inspector-image-url"; 
	var inspector_description = channel_feature_example + "_inspector-description";
	var facts = channel_feature_example + "_facts";	
	var samples = channel_feature_example + "_samples";
	var buttons = channel_feature_example + "_buttons";
	var select_channel = channel_feature_example + "_select_channel";
	var select_feature = channel_feature_example + "_select_feature";	
	var channels_ul = channel_feature_example + "_channels_ul";
	var features_ul = channel_feature_example + "_features_ul";
	var channelslt = channel_feature_example + "_channelslt";
	var featureslt = channel_feature_example + "_featureslt";
	
	//Animation variables
	var max = parseInt($(examples_num).text());
	var index = parseInt($(current_example).text().substring(7));
	var play = channel_feature_example + "_play";
	var stop = channel_feature_example + "_stop";
	var forward = channel_feature_example + "_forward";
    var backward = channel_feature_example + "_backward";

	//Displaying scrollTop
	if(localStorage.getItem("scrollTop") === null || localStorage.getItem("scrollTop") === ""){
		localStorage.setItem("scrollTop", "0");
	}
	else{
		setTimeout(function(){
			var scrollValue = parseInt(localStorage.getItem("scrollTop"));
			$(window).scrollTop(scrollValue);
		}, 50);
	}

	//Web-form behavior
	hideDDMenu(1);

	//Setting localStorage for Menu - Channels Navigation
    localStorage.setItem("last_feature", feature);

	//Validating mobile device
	if(isMobile){
		$('.web-form').hide();
		$('.mobile-form').css(
		{
			"display": "inline-block",
			"float": "none"
		}); 

		$('.inspector-web-image').hide();	
		$('.inspector-mobile-image').css(
		{
			"display": "inline-block",
			"float": "none"
		}); 

		if(max > 1){
			$(buttons).show();
			fillDot(1);
		}

		clickMobileDot();

		$('.inspector-mobile-image').scroll(function(){
			var totalwidth = $(this).width() * max + (8 * (max-1));
			var scrollPercentage = 100 * $(this).scrollLeft() / totalwidth;
			var range_size = 100/max;

			for(var i=1; i<=max; i++){
				var min_value = (range_size * (i-1));
				var max_value = (range_size * i);
				if(scrollPercentage >= min_value && scrollPercentage < max_value){
					fillDot(i);
					clickMobileDot();					
				}
			}
		});
	}

	else{
		//Showing buttons if more than one image
		if(max > 1){
			$(buttons).show();

			$(inspector_image_url).mouseenter(function(){
				index = parseInt($(current_example).text().substring(7));
				$(channel_feature_no_example + index + "_backward").show(); 
				$(channel_feature_no_example + index + "_forward").show();
			}).mouseleave(function(){
				index = parseInt($(current_example).text().substring(7));
				$(channel_feature_no_example + index + "_backward").hide(); 
				$(channel_feature_no_example + index + "_forward").hide();
			});			
			
			//Setting paging dots 		
			for(ex = 1; ex <= max; ex++){
				var div_dots = "";
				
				for(i = 1; i <= max; i++){			
					if(ex == i){
						div_dots += "<div class='inspector-oval inspector-fill' id='example" + i + "'> </div>";
					}
					else{
						div_dots += "<div class='inspector-oval' id='example" + i + "'> </div>";
					}
				}
				$(channel_feature_no_example + ex +'_dots').html(div_dots);
			}

			$('.inspector-oval').click(function() {			
				var last_ix = parseInt($(current_example).text().substring(7));
				hideLastDivs(last_ix);
				var my_index = parseInt($(this).attr("id").substring(7));
				showNextDivs(my_index);
				setDivsWithIndex(my_index);
				setMouseOver(my_index);		
			});	
		}
		else{
			$(buttons).hide();
		}		
	}

	//Defining Grid Channel scroll behavior
	clicGridDot();
	var array = $(".inspector-image-grid");
	for(i=0; i<array.length; i++){
		$(array[i]).scrollLeft(0);
	}

	$(".inspector-image-grid").scroll(function(){
		var id = $(this).attr('id');
		var channel_grid = id.split("_")[0];
		var feature_grid = id.split("_")[1];
		var max = parseInt(id.split("_")[2]);
	
		var totalwidth = ($(this).width() * max) + (8*(max-1));
		var scrollPercentage = Math.ceil(100 * $(this).scrollLeft() / totalwidth) + 1;
		var range_size = 100/max;

		for(var i=1; i<=max; i++){
			var min_value = (range_size * (i-1));
			var max_value = (range_size * i);
			if(scrollPercentage >= min_value && scrollPercentage < max_value){
				fillGridDot(channel_grid, feature_grid, i, max);
				clicGridDot();					
			}
		}
	});

	//Defining behavior for mobile-form selects.
	$(channelslt + ' option[value=' + channel + ']').prop('selected', true);
	setSelectOnChange(channelslt, "channels");
	$(featureslt + ' option[value=' + feature + ']').prop('selected', true);
	setSelectOnChange(featureslt, "features");

	//Selecting (Coloring) current Channel and Feature from unorder lists
	var temp = "li" + channel_feature + "_";
	$(channels_ul).children(temp + channel).children("a").attr("aria-checked", "true");
	$(features_ul).children(temp + feature).children("a").attr("aria-checked", "true");
	//Setting name of current Channel and Feature from unorder lists
	$(select_channel).text(channel);
	$(select_feature).text(feature);

    //Getting value from current example
    $(current_example).text(example);

    //Hide control divs 
    $('#channel_name').hide();
	$('.inspector-container').hide();
	$('.inspector-image-url').hide();
	$('.img-button').hide();
	$('.inspector-description').hide();
    $('.inspector-samples').hide();
    //$('.buttons').hide();
    $('.button').css("display", "inline-block");

    //Showing div with feature
    $(channel_feature).show();		

    //Showing div with example and facts
	$(inspector_container).show();    
    $(channel_feature_example).show();
	$(inspector_image_url).show();
	$(inspector_description).show();
    $(facts).show();
    $(samples).show();


	//Activating Control Buttons
	$('.img-button').click(function(){
		var img = $(this);
		var button_type = img.attr('class');
		var index = parseInt($(current_example).text().substring(7));		
		switch(button_type){
			case "img-button button-forward":							
				//Animating divs
				hideLastDivs(index);
				(index>=max)? index=1 : index++;
				showNextDivs(index);
				
				//Setting new variables
				setDivsWithIndex(index);
				setMouseOver(index);
				showElements(index);
				break;
			case "img-button button-backward":							
				//Animating divs
				hideLastDivs(index);
				(index <= 1)? index=max : index--;
				showNextDivs(index);
				
				//Setting new variables
				setDivsWithIndex(index);
				setMouseOver(index);
				showElements(index);
				break;                
		}        
	});

	//Activating external link icon
	$('.external-link').click(function(){
		window.open($(this).attr("href"), '_blank');
	});

	//Activating image buttons
	$(".button-backward").mouseenter(function(){
		$(this).css("background", "#0063B1");
	}).mouseleave(function(){			
		$(this).css("background", "#3A96DD");		
	});	

	$(".button-forward").mouseenter(function(){
		$(this).css("background", "#0063B1");
	}).mouseleave(function(){			
		$(this).css("background", "#3A96DD");		
	});	
	

	//Copy Code Button
	activateCodeButton(1);
	
	function activateCodeButton(index){
		var copy_div_index = channel_feature_no_example + index + "_copy_div";
		var copy_text_index = channel_feature_no_example + index + "_copy_text";
		
		$(copy_text_index).text("Copy code");

		$(copy_div_index).click(function(){
			$(copy_text_index).text("Copied");
			$(copy_div_index).css("background-color", "#003966");
		});

		$(copy_div_index).hover(function(){
			$(copy_div_index).css("background-color", "#0063B1");
		});
		$(copy_div_index).mouseleave(function(){
			$(copy_text_index).text("Copy code");
			$(copy_div_index).css("background-color", "#3A96DD");
		});		
	}
		
    function hideLastDivs(index){
		$(channel_feature_no_example + index + "_inspector-image-url").hide();
        $(channel_feature_no_example + index + "_container").hide();				
		$(channel_feature_no_example + index + "_inspector-description").hide();
		$(channel_feature_no_example + index + "_inspector-description-info").hide();
		$(channel_feature_no_example + index + "_facts").css("display", "none");
		$(channel_feature_no_example + index + "_channels_ul").children(temp + channel).children("a").attr("aria-checked", "false");
		$(channel_feature_no_example + index + "_features_ul").children(temp + feature).children("a").attr("aria-checked", "false");					
        $(channel_feature_no_example + index + "_samples").hide();
        $(channel_feature_no_example + index + "_buttons").hide();
		$(channel_feature_no_example + index + "_channelslt").hide();
		$(channel_feature_no_example + index + "_featureslt").hide();
    }

    function showNextDivs(index){
        $(channel_feature_no_example + index + "_container").show();
		$(channel_feature_no_example + index + "_inspector-image-url").show();
		$(channel_feature_no_example + index + "_inspector-description").show();
		$(channel_feature_no_example + index + "_inspector-description-info").show();
		$(channel_feature_no_example + index + "_facts").css("display", "block");
		$(channel_feature_no_example + index + "_channels_ul").children(temp + channel).children("a").attr("aria-checked", "true");
		$(channel_feature_no_example + index + "_features_ul").children(temp + feature).children("a").attr("aria-checked", "true");	
		$(channel_feature_no_example + index + "_select_channel").text(channel);
		$(channel_feature_no_example + index + "_select_feature").text(feature);			
        $(channel_feature_no_example + index + "_samples").show();
        $(channel_feature_no_example + index + "_buttons").show();
		$(channel_feature_no_example + index + "_channelslt").show();
		$(channel_feature_no_example + index + "_featureslt").show();
		$(channel_feature_no_example + index + "_channelslt" + ' option[value=' + channel + ']').prop('selected', true);
		$(channel_feature_no_example + index + "_featureslt" + ' option[value=' + feature + ']').prop('selected', true);
		setSelectOnChange(channel_feature_no_example + index + "_channelslt", "channels");
		setSelectOnChange(channel_feature_no_example + index + "_featureslt", "features");
		activateCodeButton(index);
		hideDDMenu(index);
    }

	function setDivsWithIndex(index){
		$(current_example).text("example" + index);            
		play = channel_feature_no_example + index + "_play";
		stop = channel_feature_no_example + index + "_stop";
		forward = channel_feature_no_example + index + "_forward";
		backward = channel_feature_no_example + index + "_backward";		
	}
	
	function showElements(index) {
		$(channel_feature_no_example + index + "_backward").show(); 
		$(channel_feature_no_example + index + "_forward").show();
	}

	function setMouseOver(index){
		$(channel_feature_no_example + index + "_inspector-image-url").mouseenter(function(){
			$(channel_feature_no_example + index + "_backward").show(); 
			$(channel_feature_no_example + index + "_forward").show();
		}).mouseleave(function(){
			$(channel_feature_no_example + index + "_backward").hide(); 
			$(channel_feature_no_example + index + "_forward").hide();
		});
	}

	function setSelectOnChange(select, focused){
		$(select).on('change', function() {
			var optionSelected = $(this).find("option:selected");
			var textSelected   = optionSelected.text();
			setFocused(focused);
			if(focused == "channels"){
				changeURL(textSelected, feature, example);
			}
			else{
				changeURL(channel, textSelected, example);
			}
		});			
	}

	//Filling a specific dot
	function fillDot(index){
		var div_dots = "";
		for(i = 1; i <= max; i++){			
			if(i == index){
				div_dots += "<div class='inspector-oval inspector-fill' id='example" + i + "'> </div>";
			}
			else{
				div_dots += "<div class='inspector-oval' id='example" + i + "'> </div>";
			}
		}
		$(channel_feature_no_example + '1_dots').html(div_dots);
	}

	function clickMobileDot(){
		$('.inspector-oval').click(function() {
			var exa = $(this).attr('id');
			var index = parseInt(exa.substring(7));			
			var range_size = 100/max;
			var move = ((index-1) * range_size);
			var totalwidth = ($(".inspector-mobile-image").width() * max) + (8*(max-1));
			$('.inspector-mobile-image').scrollLeft((move*totalwidth/100));
		});					
	}

	//Grid function helpers
	function fillGridDot(channel_grid, feature_grid, index, max){
		var div_dots = "";
		var dot_name = channel_grid + "_" + feature_grid + "_" + max + "_example";
		for(i = 1; i <= max; i++){
			if(i == index){
				div_dots += "<div class='inspector-oval inspector-fill' id='" + dot_name + i + "'> </div>";
			}
			else{
				div_dots += "<div class='inspector-oval' id='" + dot_name + i + "'> </div>";
			}
		}
		var div_grid_dots = "#channel_" + channel_grid + "_feature_" + feature_grid + "_" + max  + "_example_example1_dots";
		$(div_grid_dots).html(div_dots);		
	}

	function clicGridDot(){
		$('.inspector-oval').click(function() {
			var id = $(this).attr('id');
			var channel_grid = id.split("_")[0];
			var feature_grid = id.split("_")[1];
			var max = parseInt(id.split("_")[2]);
			var exa = id.split("_")[3];

			var index = parseInt(exa.substring(7));	
			var range_size = 100/max;
			var move = ((index-1) * range_size);

			var div_width = $('.inspector-image-grid').width();
			var totalwidth = (div_width * max) + (8*(max-1));
			var div_image_grid = "#" + channel_grid + "_" + feature_grid + "_" + max + "_image_grid";						
			$(div_image_grid).scrollLeft(Math.ceil(move*totalwidth/100));
		});
	}	
	
	//Hiding Menu helper
	function hideDDMenu(index){
		$(document).off("click");
		$(document).on("click", function(e) {
			var target = "#" + e.target.id;
			var sc =  channel_feature_no_example + index + "_select_channel";
			var sf = channel_feature_no_example + index + "_select_feature";
			var cul = channel_feature_no_example + index + "_channels_ul";
			var ful = channel_feature_no_example + index + "_features_ul";
			var cul_css = $(cul).css("display");
			var ful_css = $(ful).css("display");

			if(target != sc) {
				$(cul).css("display", "none");
			}
			
			if(target != sf) {
				$(ful).css("display", "none");
			}

			var outsideClick = false;
			if(e.target.nodeName == "A"){
				if(target != sc && target != sf){
					outsideClick = true;
				}
			}

			var gotopage = ((target == "#" && (cul_css == "block" || ful_css == "block")) || outsideClick); 
			
			if(!gotopage)
				e.preventDefault();
		});			
	}	
}
