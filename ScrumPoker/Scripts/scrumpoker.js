var scrumpoker = angular.module('scrumpoker', []);
scrumpoker.controller('gameController', function ($scope, $timeout) {
    var scrumpoker = $.connection.scrumPokerHub;

    // game variables
    $scope.gameActive = false;
    $scope.roomName = window.location.hash.substring(1);
    $scope.players = [];
    $scope.cards = [];
    $scope.nickname = '';
    $scope.errorLogin = '';
    $scope.selectedVote = 0;
    $scope.isPlayer = true;
    $scope.revealed = false;
    $scope.counter = 0;

    $scope.onTimeout = function() {
        $scope.counter++;
        mytimeout = $timeout($scope.onTimeout, 1000);
    };

    var mytimeout = $timeout($scope.onTimeout, 1000);

    // server init events
    scrumpoker.client.joinGame = function (game) {
        $scope.gameActive = true;
        $scope.roomName = game.Name;

        for (var i = 0; i < game.Players.length; i++) {
            addPlayer(game.Players[i]);
        }

        if (game.Role === 'Player') {
            for (var j = 0; j < game.CardValues.length; j++) {
                var crd = { value: game.CardValues[j] };
                $scope.cards.push(crd);
            }
        } else {
            $scope.isPlayer = false;
        }
        $scope.$apply();
    };

    scrumpoker.client.badUsername = function () {
        $scope.errorLogin = 'Please select another username.';
        $scope.$apply();
    };

    scrumpoker.client.removePlayer = function (playerName) {
        for (var i = $scope.players.length; i--;) {
            if ($scope.players[i].nickName === playerName) {
                $scope.players.splice(i, 1);
            }
        }
        $scope.$apply();
    };

    function addPlayer(player) {
        if (player.Role === 'Player') {
            var plyr = { nickName: player.NickName, vote: ' ' };
            $scope.players.push(plyr);
            $scope.$apply();
        }
    }

    scrumpoker.client.playerAdded = addPlayer;

    scrumpoker.client.gameReveal = function (results) {
        for (var p = 0; p < $scope.players.length; p++) {
            for (var r = 0; r < results.length; r++) {
                if (results[r].NickName === $scope.players[p].nickName) {
                    $scope.players[p].vote = results[r].Vote;
                }
            }
        }

        $scope.revealed = true;
        $scope.$apply();
    };

    scrumpoker.client.gameReset = function () {
        for (var p = 0; p < $scope.players.length; p++) {
            $scope.players[p].vote = null;
        }

        $scope.selectedVote = 0;
        $scope.revealed = false;
        $scope.counter = 0;
        $scope.$apply();
    };

    // user init events
    $.connection.hub.start().done(function () {
        $scope.joinGame = function(playerType) {
            scrumpoker.server.joinGame($scope.roomName, $scope.nickname, playerType);
        };

        $scope.castVote = function(vote) {
            scrumpoker.server.vote($scope.roomName, $scope.nickname, vote);
            $scope.selectedVote = vote;
        };

        $scope.reveal = function() {
            scrumpoker.server.reveal($scope.roomName);
        };

        $scope.reset = function() {
            scrumpoker.server.reset($scope.roomName);
        };
    });
});

scrumpoker.filter('formatTimer', function () {
    return function (input) {
        function z(n) { return (n < 10 ? '0' : '') + n; }
        var seconds = input % 60;
        var minutes = Math.floor(input / 60);
        var hours = Math.floor(minutes / 60);
        return (z(hours) + ':' + z(minutes) + ':' + z(seconds));
    };
});