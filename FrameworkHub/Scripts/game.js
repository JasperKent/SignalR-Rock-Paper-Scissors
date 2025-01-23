(function () {
    switchTo("#rps-welcome");

    let gameId = "";
    let myName = "";

    let chat = $.connection.gameHub;

    initialize();

    function initialize() {
        $("#rps-welcome button").prop("disabled", false);

        $("#rps-welcome button").click(() => register());

        $("#rps-rock").click(() => throwHand("rock"));
        $("#rps-paper").click(() => throwHand("paper"));
        $("#rps-scissors").click(() => throwHand("scissors"));

        chat.client.WaitingForPlayer = () => switchTo("#rps-waiting");

        chat.client.GameStarted = (p1, p2, id) => startGame(p1, p2, id);

        chat.client.Pending = waitingFor => pending(waitingFor);

        chat.client.Drawn =(explanation, scores) => drawn(explanation, scores);

        chat.client.Won = (winner, explanation, scores) => won(winner, explanation, scores);

        $.connection.hub.start();
    }

    function register() {
        myName = $("#rps-welcome input").val();

        chat.server.register(myName)
            .catch(err => $("#rps-error").text(err));
    }

    function throwHand(selection) {
        $("#rps-playing button").prop("disabled", true);
        $(`#rps-${selection}`).addClass("rps-highlight");

        chat.server.throw(gameId, myName, selection);
    }

    function pending(waitingFor) {
        if (myName == waitingFor)
            $("#rps-status").text("Your opponent has chosen ...");
        else
            $("#rps-status").text(`Waiting for ${waitingFor}.`);

        $("#rps-sub-status").text("");
    }

    function drawn(explanation, scores) {
        $("#rps-status").text("Draw.");
        $("#rps-sub-status").text(`(${explanation}.)`);
        $("#rps-scores").text(scores);
        $("#rps-playing button").prop("disabled", false);
        $("#rps-playing button").removeClass("rps-highlight");
    }

    function won(winner, explanation, scores) {
        if (winner == myName)
            $("#rps-status").text("You won!");
        else
            $("#rps-status").text(`${winner} won.`);

        $("#rps-sub-status").text(`(${explanation}.)`);

        $("#rps-scores").text(scores);

        $("#rps-playing button").prop("disabled", false);
        $("#rps-playing button").removeClass("rps-highlight");
    }

    function switchTo(selector) {
        $("#rps-welcome").hide();
        $("#rps-waiting").hide();
        $("#rps-playing").hide();
        $(selector).show();
    }

    function startGame(p1, p2, id) {
        switchTo("#rps-playing");

        gameId = id;

        $("#rps-other").text(p1 === myName ? p2 : p1);
    }
})();