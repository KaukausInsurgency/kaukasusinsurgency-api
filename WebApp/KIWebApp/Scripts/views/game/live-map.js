$(function () {

    // handles setting the correct left offset for the vertical game nav bar when the screen is resized
    $(window).resize(function () {
        var currentWidth = $('.live-map-layout').width();
        var minNavWidth = $('.prop-game-nav').css('--min-width').trim().replace("px", "");
        var gameNav = $('.game-nav');
        gameNav.css('left', (currentWidth - minNavWidth) + 'px');
    });
});