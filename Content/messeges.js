function select_head(id) {
    $("#messageInput").val("");
    $(".contact_head").css('background-color', '#FFF');
    $("#h_" + id).css('background-color', "#C0FFEE");
    $('#chat-with-id').html($("#h_" + id).html());
    $(".contact_body").css('display', 'none');
    $("#chat_box").css('display', 'block');
    $("#b_" + id).css('display', 'block');
}