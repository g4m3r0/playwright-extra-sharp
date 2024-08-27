using Microsoft.Playwright;
using PlaywrightExtraSharp.Helpers;
using PlaywrightExtraSharp.Models;
using Xunit.Abstractions;

namespace PlaywrightExtraSharp.Tests;

using System.Threading.Tasks;
using Xunit;
using PlaywrightExtraSharp;
using PlaywrightExtraSharp.Plugins.ExtraStealth;

public class PlaywrightExtraSharpTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public PlaywrightExtraSharpTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    private const string TestUrl = "https://abrahamjuliot.github.io/creepjs/";

    [Fact]
    public async Task GoToAsyncHeadless_WhenNavigatingToGoogleHomePage_ShouldDisplayGoogleTitle()
    {
        // Initialize PlaywrightExtra with Chromium
        var playwrightExtra = await new PlaywrightExtra(BrowserTypeEnum.Chromium)
            .Install()
            .Use(new StealthExtraPlugin())
            .LaunchAsync(new()
            {
                Headless = true
            });

        // Create a new page
        var page = await playwrightExtra.NewPageAsync(null);

        // Navigate to Google
        await page.GotoAsync("http://google.com");

        // Assert that the page title is "Google"
        var title = await page.TitleAsync();
        Assert.Equal("Google", title);
    }

    [Fact]
    public async Task CompareTrustScores_DefaultVsStealth()
    {
        // Launch default Playwright and get trust score
        var defaultTrustScore = await GetTrustScoreAsync(useStealth: false);
        _testOutputHelper.WriteLine($"Default Playwright Trust Score: {defaultTrustScore}");

        // Launch Playwright with Stealth plugin and get trust score
        var stealthTrustScore = await GetTrustScoreAsync(useStealth: true);
        _testOutputHelper.WriteLine($"Stealth Playwright Trust Score: {stealthTrustScore}");

        // Assert that the stealth trust score is greater than the default
        Assert.True(stealthTrustScore > defaultTrustScore,
            "Stealth trust score should be greater than default trust score.");
    }

    private static async Task<double> GetTrustScoreAsync(bool useStealth)
    {
        IPage page;
        if (useStealth)
        {
            // Initialize PlaywrightExtra with the Stealth plugin
            var playwrightExtra = await new PlaywrightExtra(BrowserTypeEnum.Chromium)
                .Install()
                .Use(new StealthExtraPlugin())
                .LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

            page = await playwrightExtra.NewPageAsync(null);
        }
        else
        {
            // Launch Playwright without any plugins
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright[BrowserType.Chromium]
                .LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            page = await browser.NewPageAsync();
        }

        await page.GotoAsync(TestUrl);
        await page.WaitForTimeoutAsync(10000);

        // Extract the trust score from the page
        var trustScoreElement = page.GetByText("trust score:");
        var trustScoreText = await trustScoreElement.InnerTextAsync();
        var trustScore = ParseTrustScore(trustScoreText);

        return trustScore;
    }

    private static double ParseTrustScore(string trustScoreText)
    {
        // Extract the numeric trust score from the text
        trustScoreText = trustScoreText.Replace("trust score:", "");
        var scoreString = trustScoreText.Split('%')[0].Trim();
        return double.TryParse(scoreString, out var score) ? score : 0.0;
    }
}