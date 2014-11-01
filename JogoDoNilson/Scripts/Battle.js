var debug;


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
    var image = $(card).find('.conteudo_imagem').css("background-image");
    var atk = parseInt(card.find(".ataque").html());
    var def = parseInt(card.find(".defesa").html());
    var playerMana = parseInt($(".player1 .playerManaBar").html());
    var field = $(".playerField .defenseField");
    if (cost > playerMana)
        return;

    playerMana = playerMana - cost;
    $(".player1 .playerManaBar").html(playerMana);
    field.append(buildCreature({
        id: $(".modalMenu").data("target"),
        atk: atk,
        def: def,
        image: image
    }));
    card.fadeOut();
}

function buildCreature(creature) {
    var creatureHtml = "<div class='creature' data-id=" + creature.id + " style='background-image:" + creature.image + "'>";
    creatureHtml += "<div class='fieldAttribute atk'>" + creature.atk + "</div>";
    creatureHtml += "<div class='fieldAttribute def'>" + creature.def + "</div>";
    creatureHtml += "</div>";

    return creatureHtml;
}

function attack(id) {
    var targetid = id != undefined ? id : $(".modalMenu").data("target");
    forward(id);
    //todo ajax;
}
function forward(id) {
    var creature;
    var targetid = id != undefined ? id : $(".modalMenu").data("target");
    var creature = $(".creature[data-id=" + targetid + "]");
    $(".playerField .atackField").append(creature);
    //todo ajax
}
function retreat(id) {
    var creature;
    var targetid = id != undefined ? id : $(".modalMenu").data("target");
    var creature = $(".creature[data-id=" + targetid + "]");
    $(".playerField .defenseField").append(creature);
    //todo ajax
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

    $(".playerField").click(function (ev) {
        var src = $(ev.srcElement);
        debug = src;
        var posX = ev.pageX;
        var posY = ev.pageY;

        while (!src.hasClass("creature")) {
            if (src.hasClass(".playerField") || src.find(".creature").length > 0)
                return;
            src = src.parent();
        }

        showMenu(posX, posY, src.attr("data-id"));

    })
    $(".modalMenu li").click(function () {
        hideMenu();
    });
});