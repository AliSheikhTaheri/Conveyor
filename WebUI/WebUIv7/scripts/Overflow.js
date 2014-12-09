$(function () {
    $('.button').click(function (event) {
        // Don't go the the link that was clicked, just 
        // cancel the rest of the click event
        event.preventDefault();
        
        // If there were any errors displaying hide them 
        // until we get a new one or a success message
        $(".error-message").hide("fast").text("");
        
        // Show that we're trying to send the message
        $(".sending-message").show("fast");
        
        // Post the form to the Umbraco Web API controller
        // To be found in App_Code/UmbContactController.cs
        $.ajax({
            url: "/umbraco/api/umbcontact/post/",
            data: {
                email: $("form input[name='email']").val(),
                name: $("form input[name='name']").val(),
                message: $("form textarea[name='message']").val(),
                settingsNodeId: $("form input[name='settingsNodeId']").val()
            },
            type: "POST",
            success: function () {
                // Clear the form fields after successful submit
                $("form input[name='email']").val("");
                $("form input[name='name']").val("");
                $("form textarea[name='message']").val("");
                
                // Then hide the form/sending message and show the success message
                $(".sending-message").hide("fast");
                $("form").hide("fast");
                $(".success-message").show("fast");
            },
            error: function (xhr) {
                $(".sending-message").hide("fast");
                $(".error-message").text(xhr.responseJSON.Message).show("fast");
            }
        });
    });
});
