$(document).ready(function () {
    $("#yeartag").html("Copyright {0} C.A. DUNCAN-LAUD ".replace("{0}", (new Date()).getFullYear()));
    $("#alertbox1").slideUp(300).delay(5*60*1000).fadeIn(400);

    $(document).on('mouseover', "*[title]", function () {
        $(this).tooltip();
    });

    $(window).scroll(function () {
        var top = $(document).scrollTop();
        $('.splash').css({ 'background-position': '0px -' + (top / 3).toFixed(2) + 'px' });
        (top > 50) ? $('.navbar').removeClass('navbar-transparent') : $('.navbar').addClass('navbar-transparent');
    });

    $('a[href="#"]').click(function (e) {
        e.preventDefault();
    });
});