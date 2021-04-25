var mod = angular.module('ngDuncanLaud', ['ngSanitize', 'ngRoute']);

mod.config(function ($routeProvider, $locationProvider) {
    $routeProvider
        .when("/Home", {
            templateUrl: 'views/homeView.html'
        })
        .when("/About", {
            templateUrl: 'views/aboutView.html'
        })
        .when("/Book/:bookId", {
            templateUrl: 'views/bookView.html',
            controller: 'BookController',
        })
        .otherwise({
            templateUrl: 'views/homeView.html'
        });
    //$locationProvider.html5Mode(true);
});

mod.directive("books", function () {
    return {
        templateUrl: 'views/shared/sidebar/allBooks.html'
    }
});

mod.factory('bookListFactory', function ($sce) {
    var list = [
        {
            id: 1,
            ISBN10: '1542982138',
            ISBN13: '978-1542982139',
            Name: 'Fingers And Paws',
            Pages: 44,
            Publisher: 'CreateSpace',
            publishDate: '2017-02-06T00:00:00.000Z',
            Language: 'English',
            ProductDimensionsInches: [8.5, 8.5, 0.1],
            ShippingWeightOunces: 5.0,
            SuggestedRetailPriceUSD: [11.49],
            Author: 'C.A. Duncan-Laud',
            ThumbnailURL: 'img/fingersPawsLargeSmall.jpg',
            BookImageURL: 'img/fingersPawsLarge.jpg',
            Details: "",
            AmazonId: 1542982138,
            PurchaseURL: $sce.trustAsResourceUrl('https://www.amazon.com/gp/product/1542982138'),
            DescriptionHeading: 'Fingers and Paws: And Other Poems For The Active Mind',
            DescriptionHtmlBody: $sce.trustAsHtml('Fingers and Paws and Other Poems for the Active Minds is a collection of poems all written from a child\'s perspective. These poems cover a variety of topics that will naturally spark a conversation with any child.')

        },
        {
            id: 2,
            ISBN10: '1432775944',
            ISBN13: '978-1432775940',
            Name: 'Morning Dew',
            Pages: 68,
            Publisher: 'Outskirts Press',
            publishDate: '2012-01-05T00:00:00.000Z',
            Language: 'English',
            ProductDimensionsInches: [8.6, 5.7, 0.6],
            ShippingWeightOunces: 8.8,
            SuggestedRetailPriceUSD: [11.95, 19.95],
            Author: 'C.A. Duncan-Laud',
            ThumbnailURL: 'img/bookSmall.png',
            BookImageURL: '',
            Details: "<ul class='nav nav-list'><li><b>Author: </b>C.A. Duncan-Laud</li><li><b>Softcover/Hardcover: </b>68 pages</li><li><b>Publisher: </b>Outskirts Press (January 5, 2012)</li><li><b>Language: </b>English</li><li><b>ISBN-10: </b>1432775944</li><li><b>ISBN-13: </b>978-1432775940</li><li><b>Product Dimensions: </b>8.6 x 5.7 x 0.6 inches</li><li><b>Shipping Weight:</b> 8.8 ounces</li><li><b> Suggested Retail Price: </b><br /> $11.95 (Softcover)<br /> $19.95 (Hardcover)</li></ul>",
            AmazonId: 1432775456,
            PurchaseURL: $sce.trustAsResourceUrl('https://www.amazon.com/gp/product/1432775456'),
            DescriptionHeading: 'THE WAGGING TONGUE HAS NO BONE',
            DescriptionHtmlBody: $sce.trustAsHtml('Morning Dew is a collection of verses and poems born from the Author\'s life experiences. Her poetry encompasses a myriad of emotions that will resonate with any reader.<em>"My writing was initially influenced by sadness and betrayal, which made me feel forlorn, and I was forced to change my tune for my mental health. When I started focusing on the many blessings in my life, I recognized the joy and beauty in everyday life-so now I write about love, nature, faith and hope. It doesn\'t take the hurt away, but it takes the sting out of living." </em> Inspired by God\'s promise that "I will never leave thee comfortless," these poems reflect the melody in the heart when the soul comes alive with hope and gratitude. If you find refuge in the pages of a good book, Morning Dew will be a sanctuary for your reading pleasure.')

        },
        {
            id: 3,
            ISBN10: '0',
            ISBN13: '0',
            Name: 'More Than RhymeS',
            Pages: 0,
            Publisher: '',
            publishDate: '2017-01-01T00:00:00.000Z',
            Language: 'English',
            ProductDimensionsInches: [0, 0, 0],
            ShippingWeightOunces: 0,
            SuggestedRetailPriceUSD: [0.00, 0.00],
            Author: 'C.A. Duncan-Laud',
            ThumbnailURL: 'img/CaDuncanlaud.png',
            BookImageURL: '',
            Details: "In Progress",
            AmazonId: 0,
            PurchaseURL: $sce.trustAsResourceUrl('https://www.amazon.com/gp/product/'),
            DescriptionHeading: '',
            DescriptionHtmlBody: $sce.trustAsHtml('')

        }
    ];
    return new function () {
        return list;
    }
});

mod.controller('MainController', function ($scope, $sce, $route, $routeParams, $location, bookListFactory) {
    $scope.$route = $route;
    $scope.$location = $location;
    $scope.$routeParams = $routeParams;

    $scope.title = 'DuncanLaud.com';
    $scope.artistName = 'Christine Duncan-Laud';
    $scope.aritistBio = 'was born in Jamaica West Indies. After graduating from the Moneague Teachers College in St Ann, she taught for several years in St. Mary and St. Catherine before migrating to the US in 1991, where she earned a BA in Elementary Education at Florida Atlantic University. Christine is  currently employed as a teacher in Ft Lauderdale, Florida. She has written plays for different churches, schools and other social events.';
    $scope.artistImageUrl = 'img/author263x400.jpg';
    $scope.faceBookURL = 'https://www.facebook.com/christine.duncanlaud';
    $scope.books = bookListFactory;
    $scope.articles = [
        {
            id: 1,
            imgURL: '',
            heading: 'THE WAGGING TONGUE HAS NO BONE',
            htmlBody: $sce.trustAsHtml('Morning Dew is a collection of verses and poems born from the Author\'s life experiences. Her poetry encompasses a myriad of emotions that will resonate with any reader.<em>"My writing was initially influenced by sadness and betrayal, which made me feel forlorn, and I was forced to change my tune for my mental health. When I started focusing on the many blessings in my life, I recognized the joy and beauty in everyday life-so now I write about love, nature, faith and hope. It doesn\'t take the hurt away, but it takes the sting out of living." </em> Inspired by God\'s promise that "I will never leave thee comfortless," these poems reflect the melody in the heart when the soul comes alive with hope and gratitude. If you find refuge in the pages of a good book, Morning Dew will be a sanctuary for your reading pleasure.')
        }
    ];
});

mod.controller('BookController', function ($scope, $routeParams, bookListFactory) {
    $scope.name = 'BookController';
    $scope.params = $routeParams;
    //var value = $routeParams.bookId;
    $scope.results = [];
    var _books = bookListFactory;
    for (var i = 0; i < _books.length; i++) {
        if (_books[i].ISBN10 == $routeParams.bookId) {
            $scope.results.push(_books[i]);
        }
    }
});