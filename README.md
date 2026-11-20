# EPAM2026ATTask2
PRACTICAL TASK 2: SELENIUM WEBDRIVER. Table with web elements required for executing test cases and locators for these elements:
| Element | Locator |
|---------|---------|
|    'Accept all' button in cookie banner     |    //div[@id='onetrust-button-group']/button[@id='onetrust-accept-btn-handler']     |
|     Navigation panel    |     top-navigation__row    |
|     “Carriers” link    |     Careers   |
|    ‘Start your search here’ button     |    //div[contains(@class, 'pinned-button')]//a[contains(@class, 'button-body') and contains(@href, 'careers.epam.com')]     |
|     Location in the "Location" ("All available countries") field    |     input[aria-label='Choose your country']    |
|    Filters section title     |    "Workplace type-filter-title     |
|    “Remote” option     |    //h3[@id='Workplace type-filter-title']/ancestor::fieldset//input[@name='vacancy_type-Remote']/following-sibling::label     |
|    “Keywords” field     |     search (DevTools: [name="search"])    |
|     “Search” button     |    submit_search_box_button (DevTools: [name="submit_search_box_button"])     |
|    the latest element in the list of results     |    div[class*='List_list'] > div:last-child     |
|    “View and apply” button (Vacancy title link that opens job details page, inside last job card in the search results list)     |    .//a[@data-testid='job-card-link     |
|   mentioned language is on a page      |     body    |
|         |         |
|         |         |
|         |         |
|         |         |
