# ISunWeather

Build a console app isun.exe which will show and save weather forecasts retrieved from API.
Weather forecasts should be periodically fetched every 15 seconds then printed and saved.

Console app should accept cities parameter, --cities city1, city2, ..., cityN.  
Example: isun.exe --cities Vilnius, Kaunas, Klaipėda

Please take into account that incorrect data might be entered, so the application should have
proper error handling.

Application should be architected to be able to adapt to the upcoming requirements:
1. App could become a web or desktop app
2. API could change
3. Persistent data storage could change

To fullfil point 1 placed main application logic (App.cs) in Bussiness logic layer.
Allowing for IHost to be changed to IWebHost or other if need arises.

Used dependancy injection to fullfill 2 and 3 point.
Allowing to replace API and data storage as long interface doesn't change. 

Wrote integration tests:

![image](https://github.com/Luke1453/ISunWeather/assets/43075307/6a99e104-abcc-4e61-85f0-99b47822828b)

[Google Drive link](https://drive.google.com/file/d/1u9usBwh0sW2gaA84_l-c4MczO3CHmWqf/view?usp=sharing)
