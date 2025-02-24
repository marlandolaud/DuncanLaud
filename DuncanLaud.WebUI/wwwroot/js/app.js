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
            id: 4,
            UrlSlug: 'wings_of_hope',
            ISBN10: '1',
            ISBN13: '0',
            Name: 'Wings of Hope',
            Pages: 0,
            Publisher: '',
            publishDate: '2025-02-24T00:00:00.000Z',
            Language: 'English',
            ProductDimensionsInches: [0, 0, 0],
            ShippingWeightOunces: 0,
            SuggestedRetailPriceUSD: ['TBD'], // fill in when book is on amazon
            Author: 'C.A. Duncan-Laud',
            ThumbnailURL: 'img/WingsofHopeSmall.jpg',
            BookImageURL: 'img/WingsofHope.jpg',
            BookImageURL2: '',
            Details: "",
            AmazonId: '', // fill in when book is on amazon
            PurchaseURL: $sce.trustAsResourceUrl(''), // fill in when book is on amazon
            DescriptionHeading: 'Poems and Verses that comfort, inspire, encourage, and challenge: for every season of life, in any state of mind',
            DescriptionHtmlBody: $sce.trustAsHtml('<p data-placeholder="Type or paste your content here!" style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr" id="docs-internal-guid-3d6d6630-7fff-339c-b4a3-d0c272f19ccf">On Wings of Hope is a collection of poems and verses based on real life experiences. This body of work encompasses a myriad of experiences and emotions that will resonate with any reader.</p><p style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr">“I write for my mental and spiritual health. When I focus on my blessings, I realize the joy and beauty in everyday life – so I write about matters of the heart such as love, loss, faith, friendship, struggles, and triumph anchored in hope. These themes keep me centered and fill my life with peace and joy, even when life’s circumstances dictate otherwise.</p><p style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr"><br data-cke-filler="true"></p><p style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr">Inspired by this assurance from God’s words, “We have hope as an anchor for the soul, firm and secure…” These poems reflect the melody in the heart when the soul comes alive with hope and gratitude.&nbsp;</p><p>If you find refuge in the pages of a good book,&nbsp;On Wings of Hope will be a sanctuary for your reading pleasure.</p>')

        },
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
    $scope.artistBioTitle = 'The Heart of Education and the Art of Storytelling'
    $scope.artistBio = $sce.trustAsHtml('<p>Christine, a gifted author and educator, was born on the vibrant island of Jamaica, West Indies. From a young age, she exhibited a passion for learning and storytelling. After graduating from Moneague Teachers\' College in Jamaica, she began her teaching career in the rural communities of St. Mary and St. Catherine, where she dedicated several years to shaping young minds and inspiring future generations.</p><p>In 1991, Christine’s thirst for knowledge and professional growth led her to immigrate to the United States. She settled in Florida, where she continued to pursue her passion for education by earning a Bachelor of Arts degree in Elementary Education from Florida Atlantic University. Her commitment to her students and their success was evident throughout her career, and she became a respected member of the teaching community in Ft. Lauderdale, Florida.</p><p>Throughout her tenure in the classroom, Christine also nurtured her creative side. Her love for writing blossomed as she began crafting captivating plays for churches, schools, and social events. These plays, infused with rich storytelling and meaningful themes, became a beloved part of the communities she served. Her ability to blend education and creativity resulted in works that were both entertaining and thought-provoking, further establishing her as a versatile writer.</p><p>After many years of dedicating herself to her students and writing, Christine retired from teaching and shifted her focus fully to her passion for writing. Christine’s life has been one of dedication, creativity, and a love for both education and the written word. Though retired, she remains an active presence in the world of literature, continuing to inspire and share her unique voice with others.</p>')
    $scope.emailAddress = 'christine@duncanlaud.com'
    $scope.artistImageUrl = 'img/author263x400.jpg';
    $scope.faceBookURL = 'https://www.facebook.com/christine.duncanlaud';
    $scope.books = bookListFactory;
    $scope.articles = [
        {
            id: 1,
            imgURL: '',
            heading: '',
            htmlBody: $sce.trustAsHtml('<p data-placeholder="Type or paste your content here!"><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span class="ng-binding" style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;" ng-bind-html="article.htmlBody">Hi there! I\'m Christine Duncan - Laud, a writer who’s absolutely passionate about the world of poetry. I believe poetry is a magical way to express feelings, tell stories, and discover new ideas. Through my books, I aim to share the beauty and power of poetry with readers of all ages. Whether you are a poetry lover or just starting to explore this fascinating genre, I have something for everyone. So come on in, take a look around, and let\'s discover the magic of poetry together!</span></span></p>')
        },       
         {
            id: 2,
            imgURL: 'img/W2K-On Wings of Hope-3D Book Mockup-2.png',
             heading: 'Poems and Verses that comfort, inspire, encourage, and challenge: for every season of life, in any state of mind',
             htmlBody: $sce.trustAsHtml('<p data-placeholder="Type or paste your content here!"><span data-ck-unsafe-element="meta" charset="utf-8"></span></p><p style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr" id="docs-internal-guid-3d6d6630-7fff-339c-b4a3-d0c272f19ccf"><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span style="font-style:normal;font-variant:normal;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;"><strong>On Wings of Hope</strong></span><span style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;"> is a collection of poems and verses based on real life experiences. This body of work encompasses a myriad of experiences and emotions that will resonate with any reader.</span></span></p><p style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr"><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;">“I write for my mental and spiritual health. When I focus on my blessings, I realize the joy and beauty in everyday life – so I write about matters of the heart such as love, loss, faith, friendship, struggles, and triumph anchored in hope. These themes keep me centered and fill my life with peace and joy, even when life’s circumstances dictate otherwise.</span></span></p><p><br data-cke-filler="true"></p><p style="line-height:1.2;margin-bottom:0pt;margin-top:0pt;" dir="ltr"><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;">Inspired by this assurance from God’s words, “We have hope as an anchor for the soul, firm and secure…” These poems reflect the melody in the heart when the soul comes alive with hope and gratitude.&nbsp;</span></span></p><p><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;">If you find refuge in the pages of a good book,&nbsp;</span><span style="font-style:normal;font-variant:normal;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;"><strong>On Wings of Hope</strong></span><span style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;"> will be a sanctuary for your reading pleasure.</span></span></p>')
        },
          {
            id: 3,
            imgURL: 'img/W2K-More than Rhymes-3D Book Mockup-2-Rev1.png',
            heading: 'More than Rhymes',
              htmlBody: $sce.trustAsHtml('<p data-placeholder="Type or paste your content here!"><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span class="ng-binding" style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;" ng-bind-html="article.htmlBody">More than Rhymes is a collection of poems with real world connections. These poems include topics such as self-awareness, mental health, family, historical events, patriotism, and interesting people. The poems are educational as well as entertaining and brimming with rich vocabulary.</span></span><br><br><span style="background-color:transparent;color:#000000;font-family:Calibri,sans-serif;font-size:12pt;"><span class="ng-binding" style="font-style:normal;font-variant:normal;font-weight:400;text-decoration:none;vertical-align:baseline;white-space:pre-wrap;" ng-bind-html="article.htmlBody">More than Rhymes also has a reference section which will help to enhance each reader\'s experience.</span ></span ></p > ')
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