function showMenu(X, Y, target) {
    var menu = $(".modalMenu");
    menu.data("target", target);
    menu.css("top", Y + "px");
    menu.css("left", X + "px");
    menu.fadeIn();
}
function hideMenu() {
    var menu = $(".modalMenu");
    menu.fadeOut();
}

function putCard(sender) {
    var card = $(".cardWrapper[data-id=" + $(".modalMenu").data("target") + "]");
    var cost = parseInt(card.find(".custo_num").html());
    var atk = parseInt(card.find(".ataque").html());
    var def = parseInt(card.find(".defesa").html());
    var playerMana = parseInt($(".player1 .playerManaBar").html());
    var field = $(".playerField .defenseField");
    if (cost > playerMana)
        return;

    playerMana = playerMana - cost;
    $(".player1 .playerManaBar").html(playerMana);
    field.append("<div style='background:lightgray;width:25px;height:25px'><div>" + atk + "</div><div>" + def + "</div></div>");
    card.fadeOut();
}

$(document).ready(function () {
    $(".playerHand").click(function (ev) {
        var src = $(ev.srcElement);
        var posX = ev.pageX;
        var posY = ev.pageY;
        while (!src.hasClass("cardWrapper")) {
            if (src.hasClass(".playerHand") || src.find(".cardWrapper").length > 0)
                return;
            src = src.parent();
        }

        showMenu(posX, posY, src.attr("data-id"));
    });

    $(".modalMenu li").click(function () {
        hideMenu();
    });
});