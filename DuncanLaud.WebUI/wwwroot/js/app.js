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
            id: 3,
            UrlSlug: 'more_than_rhymes',
            ISBN10: '1',
            ISBN13: '0',
            Name: 'More Than Rhymes',
            Pages: 0,
            Publisher: '',
            publishDate: '2023-02-24T00:00:00.000Z',
            Language: 'English',
            ProductDimensionsInches: [0, 0, 0],
            ShippingWeightOunces: 0,
            SuggestedRetailPriceUSD: [14.99],
            Author: 'C.A. Duncan-Laud',
            ThumbnailURL: 'img/MoreThanRhymesSmall.jpg',
            BookImageURL: 'img/MoreThanRhymes1.jpg',
            BookImageURL2: 'img/MoreThanRhymes2.jpg',
            Details: "",
            AmazonId: 'B0BWS7F8B7',
            PurchaseURL: $sce.trustAsResourceUrl('https://www.amazon.com/dp/B0BWS7F8B7'),
            DescriptionHeading: 'Captivating Collection of Educational and Entertaining Poems',
            DescriptionHtmlBody: $sce.trustAsHtml('"More than Rhymes" is a captivating collection of poetry that skillfully intertwines real-world connections with lyrical language. Each poem offers an engaging blend of education and entertainment, making it a perfect read for all ages. From exploring historical events to celebrating the wonders of nature, this collection is brimming with rich vocabulary and stunning imagery that will leave you mesmerized. So dive into this book and discover the beauty and power of poetry like never before!')

        },
        {
            id: 4,
            UrlSlug: 'fingers_Paws',
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
            BookImageURL2: 'img/fingersPawsLarge2.jpg',
            Details: "",
            AmazonId: '1542982138',
            PurchaseURL: $sce.trustAsResourceUrl('https://www.barnesandnoble.com/w/fingers-and-paws-christine-laud/1140803680?ean=9781649613363'),
            DescriptionHeading: 'Fingers and Paws: And Other Poems For The Active Mind',
            DescriptionHtmlBody: $sce.trustAsHtml('Fingers and Paws and Other Poems for the Active Minds is a collection of poems all written from a child\'s perspective. These poems cover a variety of topics that will naturally spark a conversation with any child.')

        },
        {
            id: 1,
            UrlSlug: 'morning_dew',
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
            ThumbnailURL: 'img/bookSmall.jpg',
            BookImageURL: 'img/bookLarge.jpg',
            BookImageURL2: '',
            Details: "",
            AmazonId: '',
            PurchaseURL: $sce.trustAsResourceUrl('https://www.amazon.com/404'),
            DescriptionHeading: 'THE WAGGING TONGUE HAS NO BONE',
            DescriptionHtmlBody: $sce.trustAsHtml('Morning Dew is a collection of verses and poems born from the Author\'s life experiences. Her poetry encompasses a myriad of emotions that will resonate with any reader.<em>"My writing was initially influenced by sadness and betrayal, which made me feel forlorn, and I was forced to change my tune for my mental health. When I started focusing on the many blessings in my life, I recognized the joy and beauty in everyday life-so now I write about love, nature, faith and hope. It doesn\'t take the hurt away, but it takes the sting out of living." </em> Inspired by God\'s promise that "I will never leave thee comfortless," these poems reflect the melody in the heart when the soul comes alive with hope and gratitude. If you find refuge in the pages of a good book, Morning Dew will be a sanctuary for your reading pleasure.')

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
    $scope.aritistBio = '@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@'
    $scope.emailAddress = 'christine@duncanlaud.com'
    $scope.artistImageUrl = 'img/author263x400.jpg';
    $scope.faceBookURL = 'https://www.facebook.com/christine.duncanlaud';
    $scope.books = bookListFactory;
    $scope.articles = [
        {
            id: 1,
            imgURL: '',
            heading: '',
            htmlBody: $sce.trustAsHtml('Welcome to my website! My name is Christine Duncan-Laud, I am a passionate writer who loves to explore the world of poetry. Through my books, I aim to share the beauty and power of poetry with readers of all ages. Whether you are a poetry lover or just starting to explore this fascinating genre, I have something for everyone. So come on in, take a look around, and discover the magic of poetry with me!')
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
        if (_books[i].UrlSlug == $routeParams.bookId) {
            $scope.results.push(_books[i]);
        }
    }
});
