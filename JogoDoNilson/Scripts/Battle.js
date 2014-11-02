var player = {
    isTurn: false,
    turnPhase: 0
}


function showMenu(X, Y, target, menuTypeClass) {

    if (!player.isTurn || player.turnPhase > 2)
        return;

    var menu = $(".modalMenu");
    menu.find("li").each(function (i, obj) {
        if ($(obj).hasClass(menuTypeClass))
            $(obj).css("display", "block");
        else
            $(obj).css("display", "none");
    });
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
    var creature = {
        id: $(".modalMenu").data("target"),
        atk: atk,
        def: def,
        image: image
    };
    playerMana = playerMana - cost;

    $.ajax({
        url: "/battle/PutCardInField/",
        data: { cardId: creature.id },
        success: function (result) {
            if (result == '1') {
                $(".player1 .playerManaBar").html(playerMana);
                field.append(buildCreatureHtml(creature));
                card.fadeOut();
            }
        },
        error: function (data) {
            console.log(data);
        }
    });

}

function buildCreatureHtml(creature) {
    var creatureHtml = "<div class='creature' data-id=" + creature.id + " style='background-image:" + creature.image + "'>";
    creatureHtml += "<div class='fieldAttribute atk'>" + creature.atk + "</div>";
    creatureHtml += "<div class='fieldAttribute def'>" + creature.def + "</div>";
    creatureHtml += "</div>";

    return creatureHtml;
}

function finishAttack() {
    var attackers = $(".creature.used");
    var attackIds = [];

    attackers.each(function (i, obj) {
        attackIds.push($(obj).attr("data-id"));
    });

    $.ajax({
        url: "/battle/ChooseAttackers/",
        data: JSON.stringify({ cardIds: attackIds }),
        success: function (result) {
            if (result == 1) {
                //todo wait for defense;
            }
        },
        error: function (data) {
            console.log(data);
        }
    });
}
function attack(id) {
    var targetid = id != undefined ? id : $(".modalMenu").data("target");
    var creature = $(".creature[data-id=" + targetid + "]");
    if(forward(id)){
        creature.addClass("used");
    }
}
function forward(id) {
    var creature;
    var targetid = id != undefined ? id : $(".modalMenu").data("target");
    var creature = $(".creature[data-id=" + targetid + "]");

    $.ajax({
        url: "/battle/MoveCardToAtackField/",
        data: { cardId: targetid },
        success: function (result) {
            if (result == 1) {
                $(".playerField .atackField").append(creature);
                return true;
            }
        },
        error: function (data) {
            console.log(data);
            return false;
        }
    });
    return false;

}
function retreat(id) {
    var creature;
    var targetid = id != undefined ? id : $(".modalMenu").data("target");
    var creature = $(".creature[data-id=" + targetid + "]");


    $.ajax({
        url: "/battle/MoveCardToDefenseField/",
        data: { cardId: targetid },
        success: function (result) {
            if (result == 1) {
                creature.removeClass("used");
                $(".playerField .defenseField").append(creature);
            }
        },
        error: function (data) {
            console.log(data);
        }
    });
}
function drawCard() {
    $.ajax({
        url: "/battle/DrawCard/",
        success: function (result) {
            $(".playerHand").append(result);
        },
        error: function (data) {
            console.log(data);
        }
    });
}


function putComputerCards(data) {
    $(data).each(function (i, obj) {
        var html = buildCreatureHtml(new {
            id: obj.Id,
            atk: obj.Ataque,
            def: obj.Defesa,
            image: ''
        });
        $(".computerField .defenseField").append(html);
    });
}


$(document).ready(function () {


    var flipTimer = 500;
    var cardLeft = 200;

    $('.cardFlip').each(function () {
        var card = $(this);
        var cardWrapper = card.parent().parent();

        setTimeout(function () {

            setTimeout(function () {
                card.removeClass('flipped')}, 50
            );

            cardWrapper.css('top', '200px');
            cardWrapper.css('left', cardLeft + 'px');

            cardLeft = cardLeft + 150;
        }, flipTimer);

        flipTimer += 300;
    });

    $(".playerHand").click(function (ev) {
        var src = $(ev.srcElement);
        var posX = ev.pageX;
        var posY = ev.pageY;
        while (!src.hasClass("cardWrapper")) {
            if (src.hasClass(".playerHand") || src.find(".cardWrapper").length > 0)
                return;
            src = src.parent();
        }

        showMenu(posX, posY, src.attr("data-id"), "jsHand");
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

        var menuType = src.parent().hasClass("defenseField") ? "jsDef" : "jsAtk";
        showMenu(posX, posY, src.attr("data-id"), menuType);

    })
    $(".modalMenu li").click(function () {
        hideMenu();
    });
    $(".finishButton").click(function () {
        finishAttack();
    });
    setInterval(function () {
        if (player.isTurn || (player.turnPhase > 2 && !player.isTurn))
            return;

        $.ajax({
            url: "/battle/VerifyPhase/",
            type: "POST",
            success: function (result) {
                player.isTurn = result.isYourTurn;
                player.turnPhase = result.phase;

                if (result.isYourTurn) {
                    switch (result.phase) {
                        case 1:
                            drawCard();
                        default:
                            break;
                    }
                }
                else {
                    switch (result.phase) {
                        case 2:
                            putComputerCards(result.data);
                            break;
                        default:
                            break;
                    }
                }
            },
            error: function (result) {

            }
        })
    }, 6000);
});