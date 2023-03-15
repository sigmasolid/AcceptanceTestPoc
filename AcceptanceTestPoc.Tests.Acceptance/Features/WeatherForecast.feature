Feature: WeatherForecast test
  
  Scenario: WeatherForecast fetching 
    When I call the Weather forecast API 
    Then I get a 5 random weather forecasts