using PlaywrightExtraSharp.Models;

namespace PlaywrightExtraSharp.Tests;

using System.Threading.Tasks;
using Xunit;
using PlaywrightExtraSharp;
using PlaywrightExtraSharp.Plugins.ExtraStealth;

public class PlaywrightExtraSharpTests
{
    [Fact]
    public async Task GoToAsyncHeadless_WhenNavigatingToGoogleHomePage_ShouldDisplayGoogleTitle()
    {
        // Initialize PlaywrightExtra with Chromium
        var playwrightExtra = await new PlaywrightExtra(BrowserTypeEnum.Chromium)
            .Install()
            .Use(new StealthExtraPlugin())
            .LaunchAsync(new ()
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

        // Close the browser
        await playwrightExtra.CloseAsync();
    }
}