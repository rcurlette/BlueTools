// Created by Robert Curlette, www.curlette.com

$(document).ready(function() {
    var uri = getParameterByName('Uri')
	$('#uri').val(uri)
	$('#success').hide()
    $('#componentList').hide()
    $('#inprogress').hide()
    $('#error').hide()
	$('#uri').attr('readonly', 'readonly')
	$('#title').focus()
	$('#spinner').hide()
	$('#componentListHeading').hide()
		
	// Disable filename field for Components	
	if (isTridionPage(uri) == false){
         $('#filename').attr('readonly', 'readonly')
    }
	
    $("#btnCopy").click(function (e) {
        //This line will prevent the form from submitting
        e.preventDefault()
		$('#btnCopy').attr('disabled', 'disabled')
		$('#spinner').show()
		$('#inprogress').show()
        dataString = $('#frmCopy').serialize()  //jQuery.param.querystring(window.location.href);
        // For local debugging... url: "http://localhost:61860/api/blueCopy",
        // test the URL in a browser first...
		$.ajax({
			type: "POST",
			url: "http://TridionDev2011:9090/Tridion2011ServiceStack/api/blueCopy",
			data: dataString,
			dataType: "jsonp",
			success: function (data) {
				$('#componentListHeading').show()
				$('#spinner').hide()
				
				$.each(data, function () {
					$('<li>' + this.sourceTitle + '<span class="icon-arrow-right" style="padding-right:4px;padding-left:4px;"></span>' + this.title + '</li>').appendTo("#componentList")

					if (data.error != null) {
						$('#error').html(data.error)
						$('#error').show()
					}
					else {
						$('#inprogress').hide()
						$('#success').show()
						$('#componentList').show()
					}
				});
				
			},
			error: function (request, status, error) {
				alert(request.responseText)
				$('#error').text(status + "," + request.responseText + "," + error)
				$('#error').show()
			}
        });
    });
	
	function isTridionPage(uri) {
        if (uri.indexOf("-64") == (uri.length - 3))
            return true
		else
			return false
    }

	function getParameterByName(name)
	{
	  name = name.replace(/[\[]/, "\\\[").replace(/[\]]/, "\\\]")
	  var regexS = "[\\?&]" + name + "=([^&#]*)"
	  var regex = new RegExp(regexS)
	  var results = regex.exec(window.location.search)
	  if(results == null)
		return ""
	  else
		return decodeURIComponent(results[1].replace(/\+/g, " "))
	}
});