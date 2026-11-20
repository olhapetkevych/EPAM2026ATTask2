using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace EPAM2026ATTask2
{
    [TestFixture]
    public class Tests
    {
        private IWebDriver _driver = null!;
        private WebDriverWait _wait = null!;
        private string _baseUrl = null!;
        private int _implicitWaitSeconds;
        private int _explicitWaitSeconds;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(TestContext.CurrentContext.TestDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _baseUrl = config["TestSettings:BaseUrl"]
                ?? throw new InvalidOperationException("BaseUrl is missing in appsettings.json");
            _implicitWaitSeconds = int.Parse(config["TestSettings:ImplicitWaitSeconds"] ?? "0");
            _explicitWaitSeconds = int.Parse(config["TestSettings:ExplicitWaitSeconds"] ?? "15");
        }

        [SetUp]
        public void Setup()
        {
            var options = new ChromeOptions();
            options.AddArgument("--disable-notifications");
            options.AddArgument("--disable-popup-blocking");
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            options.PageLoadStrategy = PageLoadStrategy.Normal;

            _driver = new ChromeDriver(options);
            _driver.Manage().Window.Maximize();
            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_implicitWaitSeconds);
            _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_explicitWaitSeconds));
        }

        [Test]
        [TestCase("All available countries", "C#")]
        [TestCase("All available countries", "Java")]
        [TestCase("All available countries", "Python")]
        [TestCase("All available countries", "TypeScript")]
        public void SearchPositionByCriteria_ShouldDisplayKeywordOnVacancyPage(string location, string programmingLanguage)
        {
            // 1. Navigate to https://www.epam.com/
            _driver.Navigate().GoToUrl(_baseUrl);

            // 2. Click 'Accept all' button in cookie banner [XPath Relative path]
            AcceptCookiesIfPresent();

            // Перевірка видимості навігаційної панелі [ClassName locator]
            _wait.Until(d => IsElementDisplayed(By.ClassName("top-navigation__row")));

            // 3. Find a link "Careers" and click on it [LinkText locator]
            IWebElement careersLink = WaitUntilClickable(By.LinkText("Careers"));
            careersLink.Click();

            // 4. Click 'Start your search here' button [XPath locator with contains()] (PartialLinkText locator не вийшло зробити)
            By startSearchLocator = By.XPath(
    "//div[contains(@class, 'pinned-button')]//a[contains(@class, 'button-body') and contains(@href, 'careers.epam.com')]");

            IWebElement startSearchButton = WaitUntilClickable(startSearchLocator);
            ScrollToElement(startSearchButton);
            startSearchButton.Click();

            // Повторна перевірка cookie-банера на піддомені careers.epam.com
            AcceptCookiesIfPresent();

            // 5. Select location in the "Location" ("All available countries") field [CSS locator + XPath with operator]
            IWebElement locationInput = WaitUntilClickable(By.CssSelector("input[aria-label='Choose your country']"));
            locationInput.Click();
            locationInput.Clear();
            locationInput.SendKeys(location);

            // XPath із оператором 'and' та атрибутом role='option'
            By locationOptionLocator = By.XPath($"//div[@role='option' and normalize-space()='{location}']");
            IWebElement locationOption = WaitUntilClickable(locationOptionLocator);
            locationOption.Click();

            // Перевірка заголовка секції фільтрів [ID locator]
            _wait.Until(d => IsElementDisplayed(By.Id("Workplace type-filter-title")));

            // 7. Select the option "Remote" [XPath locator with axes: ancestor + following-sibling]
            By remoteLabelLocator = By.XPath(
                "//h3[@id='Workplace type-filter-title']/ancestor::fieldset//input[@name='vacancy_type-Remote']/following-sibling::label");
            IWebElement remoteLabel = WaitUntilClickable(remoteLabelLocator);
            ScrollToElement(remoteLabel);
            remoteLabel.Click();

            // 6. Write the name of programming language in the field "Keywords" [Name locator]
            IWebElement keywordsInput = WaitUntilClickable(By.Name("search"));
            keywordsInput.Click();
            keywordsInput.Clear();
            keywordsInput.SendKeys(programmingLanguage);

            // 8. Click on the button "Search" [Name locator]
            IWebElement searchButton = WaitUntilClickable(By.Name("submit_search_box_button"));
            searchButton.Click();

            // Очікуємо оновлення результатів пошуку
            WaitForSearchResultsUpdate();

            // 9. Find the latest element in the list of results [CSS locator with pseudo-class :last-child]
            IWebElement latestJobCard = _wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector("div[class*='List_list'] > div:last-child"));
                return el.Displayed ? el : null;
            });
            ScrollToElement(latestJobCard);

            // 10. Click on the vacancy link inside the latest card [XPath Relative path]
            IWebElement vacancyLink = latestJobCard.FindElement(By.XPath(".//a[@data-testid='job-card-link']"));
            string expectedHref = vacancyLink.GetDomAttribute("href") ?? vacancyLink.GetAttribute("href") ?? string.Empty;
            ScrollToElement(vacancyLink);
            vacancyLink.Click();

            _wait.Until(d =>
                d.Url.Contains("/vacancy/", StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrEmpty(expectedHref) && d.Url.Contains(expectedHref, StringComparison.OrdinalIgnoreCase)));

            // 11. Validate that the programming language mentioned in the step above is on a page [TagName locator]
            IWebElement pageBody = _wait.Until(d =>
            {
                var body = d.FindElement(By.TagName("body"));
                return body.Displayed ? body : null;
            });

            Assert.That(
                pageBody.Text,
                Does.Contain(programmingLanguage).IgnoreCase,
                $"Очікувалося, що на сторінці вакансії ({_driver.Url}) буде знайдено '{programmingLanguage}'.");
        }

        private IWebElement WaitUntilClickable(By locator)
        {
            try
            {
                return _wait.Until(d =>
                {
                    try
                    {
                        var element = d.FindElement(locator);
                        return (element.Displayed && element.Enabled) ? element : null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }
                });
            }
            catch (WebDriverTimeoutException ex)
            {
                TestContext.Out.WriteLine($"Timeout waiting for {locator}. Current URL: {_driver.Url} | Title: {_driver.Title}");
                throw new WebDriverTimeoutException(
                    $"Timed out after {_explicitWaitSeconds}s waiting for {locator} on URL '{_driver.Url}' (Title: '{_driver.Title}').",
                    ex);
            }
        }

        private bool IsElementDisplayed(By locator)
        {
            try
            {
                return _driver.FindElement(locator).Displayed;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }

        private void AcceptCookiesIfPresent()
        {
            By acceptBtnLocator = By.XPath("//div[@id='onetrust-button-group']/button[@id='onetrust-accept-btn-handler']");
            var shortWait = new WebDriverWait(_driver, TimeSpan.FromSeconds(3));

            _driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
            try
            {
                IWebElement acceptCookiesBtn = shortWait.Until(d =>
                {
                    var btn = d.FindElement(acceptBtnLocator);
                    return (btn.Displayed && btn.Enabled) ? btn : null;
                });

                acceptCookiesBtn.Click();
                shortWait.Until(d => d.FindElements(acceptBtnLocator).Count == 0 || !IsElementDisplayed(acceptBtnLocator));
            }
            catch (WebDriverTimeoutException)
            {
                // Банер не з'явився або вже був прийнятий
            }
            finally
            {
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_implicitWaitSeconds);
            }
        }

        private void ScrollToElement(IWebElement element)
        {
            ((IJavaScriptExecutor)_driver).ExecuteScript(
                "arguments[0].scrollIntoView({block: 'center', inline: 'nearest'});", element);
        }

        private void WaitForSearchResultsUpdate()
        {
            _wait.Until(d =>
            {
                var items = d.FindElements(By.CssSelector("div[class*='List_list'] > div"));
                return items.Count > 0 && items[0].Displayed;
            });
        }

        [TearDown]
        public void TearDown()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}