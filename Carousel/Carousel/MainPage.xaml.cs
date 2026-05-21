using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Uno.Toolkit.UI;

namespace CarouselDemo;

public sealed partial class MainPage : Page
{
    private readonly Random _random = new();
    private List<PhotoItem> _bindingPhotos = new();

    public MainPage()
    {
        this.InitializeComponent();
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        SetupBasicCarousel();
        SetupVerticalCarousel();
        SetupTemplatedCarousel();
        SetupAutoPlayCarousel();
        SetupNoLoopCarousel();
        SetupFadeCarousel();
        SetupBindingCarousel();
        SetupEdgeCaseCarousels();
    }

    private void SetupBasicCarousel()
    {
        var photos = new List<PhotoItem>
        {
            new("https://picsum.photos/seed/carousel1/1080/720", "Mountain Landscape"),
            new("https://picsum.photos/seed/carousel2/1080/720", "Ocean Sunset"),
            new("https://picsum.photos/seed/carousel3/1080/720", "Forest Trail"),
            new("https://picsum.photos/seed/carousel4/1080/720", "City Skyline"),
            new("https://picsum.photos/seed/carousel5/1080/720", "Desert Dunes"),
        };

        BasicCarousel.ItemsSource = photos;
        BasicCarousel.SelectionChanged += (s, args) =>
        {
            BasicCarouselStatus.Text = $"{args.NewIndex + 1} / {photos.Count}";
        };
        BasicCarouselStatus.Text = $"1 / {photos.Count}";
    }

    private void SetupVerticalCarousel()
    {
        var photos = new List<PhotoItem>
        {
            new("https://picsum.photos/seed/vert1/1080/720", "Autumn Leaves"),
            new("https://picsum.photos/seed/vert2/1080/720", "Snowy Peaks"),
            new("https://picsum.photos/seed/vert3/1080/720", "Tropical Beach"),
            new("https://picsum.photos/seed/vert4/1080/720", "Rolling Hills"),
        };

        VerticalCarousel.ItemsSource = photos;
    }

    private void SetupTemplatedCarousel()
    {
        var slides = new List<CardItem>
        {
            new("https://picsum.photos/seed/card1/1080/720",
                "Explore Nature",
                "Discover breathtaking landscapes from around the world"),
            new("https://picsum.photos/seed/card2/1080/720",
                "Urban Architecture",
                "Modern cityscapes and iconic buildings captured in stunning detail"),
            new("https://picsum.photos/seed/card3/1080/720",
                "Wildlife",
                "Intimate portraits of animals in their natural habitats"),
            new("https://picsum.photos/seed/card4/1080/720",
                "Abstract Art",
                "Creative compositions that challenge perception"),
        };

        TemplatedCarousel.ItemsSource = slides;
    }

    private void SetupAutoPlayCarousel()
    {
        var urls = new List<string>
        {
            "https://picsum.photos/seed/auto1/1080/720",
            "https://picsum.photos/seed/auto2/1080/720",
            "https://picsum.photos/seed/auto3/1080/720",
            "https://picsum.photos/seed/auto4/1080/720",
            "https://picsum.photos/seed/auto5/1080/720",
        };

        AutoPlayCarousel.ItemsSource = urls;
        AutoPlayCarousel.AutoPlayInterval = TimeSpan.FromSeconds(3);
    }

    private void SetupNoLoopCarousel()
    {
        var photos = new List<PhotoItem>
        {
            new("https://picsum.photos/seed/noloop1/1080/720", "First - cannot go back"),
            new("https://picsum.photos/seed/noloop2/1080/720", "Middle"),
            new("https://picsum.photos/seed/noloop3/1080/720", "Last - cannot go forward"),
        };

        NoLoopCarousel.ItemsSource = photos;
    }

    private void SetupFadeCarousel()
    {
        FadeCarousel.ItemTransition = new FadeTransition();

        var photos = new List<PhotoItem>
        {
            new("https://picsum.photos/seed/fade1/1080/720", "Cross-fade transition"),
            new("https://picsum.photos/seed/fade2/1080/720", "Smooth opacity blend"),
            new("https://picsum.photos/seed/fade3/1080/720", "Elegant reveal"),
        };

        FadeCarousel.ItemsSource = photos;
    }

    private void SetupBindingCarousel()
    {
        _bindingPhotos = new List<PhotoItem>
        {
            new("https://picsum.photos/seed/bind1/1080/720", "Slide 1"),
            new("https://picsum.photos/seed/bind2/1080/720", "Slide 2"),
            new("https://picsum.photos/seed/bind3/1080/720", "Slide 3"),
            new("https://picsum.photos/seed/bind4/1080/720", "Slide 4"),
            new("https://picsum.photos/seed/bind5/1080/720", "Slide 5"),
        };

        BindingCarousel.ItemsSource = _bindingPhotos;

        // Set slider range
        BindingSlider.Maximum = _bindingPhotos.Count - 1;
        BindingSlider.Value = 0;
        UpdateBindingIndexText(0);

        // Two-way sync: Slider -> Carousel
        BindingSlider.ValueChanged += (s, args) =>
        {
            var index = (int)args.NewValue;
            if (BindingCarousel.SelectedIndex != index)
            {
                BindingCarousel.GoTo(index);
            }
        };

        // Two-way sync: Carousel -> Slider
        BindingCarousel.SelectionChanged += (s, args) =>
        {
            BindingSlider.Value = args.NewIndex;
            UpdateBindingIndexText(args.NewIndex);
        };
    }

    private void UpdateBindingIndexText(int index)
    {
        BindingIndexText.Text = $"Item {index + 1} of {_bindingPhotos.Count}";
    }

    private void SetupEdgeCaseCarousels()
    {
        // Empty carousel - just set null/empty source
        EmptyCarousel.ItemsSource = new List<string>();

        // Single-item carousel
        SingleItemCarousel.ItemsSource = new List<PhotoItem>
        {
            new("https://picsum.photos/seed/single1/1080/720", "Only item - no navigation possible"),
        };
    }

    private void OnGoToFirstClicked(object sender, RoutedEventArgs e) => BindingCarousel.GoTo(0);

    private void OnGoToLastClicked(object sender, RoutedEventArgs e)
    {
        if (_bindingPhotos.Count > 0)
            BindingCarousel.GoTo(_bindingPhotos.Count - 1);
    }

    private void OnGoToRandomClicked(object sender, RoutedEventArgs e)
    {
        if (_bindingPhotos.Count > 1)
        {
            int current = BindingCarousel.SelectedIndex;
            int next;
            do
            {
                next = _random.Next(0, _bindingPhotos.Count);
            } while (next == current);

            BindingCarousel.GoTo(next);
        }
    }
}

public record PhotoItem(string ImageUrl, string Caption);

public record CardItem(string ImageUrl, string Title, string Description);
