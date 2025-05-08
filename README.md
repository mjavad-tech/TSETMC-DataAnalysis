
# TSETMC Data Analysis - C# WPF Application




## Table of Contents

 - [Overview](#Overview)
 - [Features](#Features)
 - [App View](#App-View)
 - [Getting Started](#Getting-Started)
 - [Technologies Used](#Technologies-Used)
 - [Why C# for This Project?](Why-C#-for-This-Project-)
 - [Limitation](#Limitation)
 - [Contribution](#Contribution)
 - [Future Improvements & Application](#Future-Improvements---Application)
 - [Authors](#Authors)


## Overview

This desktop application is built with C# and WPF that fetches live stock options data from Iran's official stock market website(https://www.tsetmc.com/). It analyzes each stock option using various trading strategies such as **covered call**, **conversion** and **long straddle**, calculating potential profitability and presenting the results in a clean, tabular UI.


## Features

- **Live Data Fetching** from the official Iranian stock price source
- **Options Strategy Calculations**, including :
    - Covered Call
    - Conversion
    - Long Straddle
- **User-Friendly WPF Interface** with sortable, tabular data display
- **Clear Data Presentation** to assist investment decision-making
- Including **all important information** about each stock 
- **Persian Text Normalization** (converts Arabic variants for consistency)
- Including **Culture-Specific Parsing** (Persian/Iranian localization)


## App View


![App Screenshot](screenshots/view.gif)


## Getting Started

Follow these instructions to run the project locally and understand how to use it.
### Prerequisites
- Windows OS
- Visual Studio 2022 (with .NET desktop development workload)
- [.Net Framework 4.8.1](https://dotnet.microsoft.com/)
- Internet connection to fetch live stock data 

### Clone and Run

```bash
  git Clone
  https://github.com/mjavad-tech/TSETMC-DataAnalysis.git 
```

### Open in Visual Studio

1. Open **Visual Studio**
2. Click on **"Open a project or solution"**
3. Navigate to the cloned project folder
4. Open the `.sln` (solution) file

### Run the application
- Press **F5** or click **Start** to build and launch the application
- The application will fetch stock from the Iranian stock market website (TSETMC.com)
- Once loaded, the data will be processed to calculate profitability based on various strategies (e.g., **Covered Call**, **Conversion** and **Long Straddle**)
- Results will be displayed in a **tabular user interface**. (after some seconds)
## Technologies Used

- **C#/ .NET**
- **WPF (Windows Presentation Foundation)**



## Why C# for This Project?

While Python is popular for data analysis, this project uses C# for several key reasons:
- **High Performance** : C# offers faster raw data processing for real-time calculations compared to Python, especially in desktop applications.
- **Rich UI Support** : WPF (Windows Presentation Foundation) allows for creating responsive and modern desktop interfaces - something that python struggles with natively.
- **Strong Typing & Tooling** : C# and Visual Studio provide robust tooling and compile-time checks, making the development process more stable for larger applications.


## Limitation


- Only compatible with Iranian Stock market structure
- Currently built for educational and local use - not intended for production-level trading
- This project is not affiliated with or endorsed by "TSETMC.com". All data used is publicly available on the TSETMC website. This project was for educational and research purposes only.



## Contribution

This project was developed as an academic project. While it may not be perfect, but contributions, improvements, or discussions are welcome!



## Future Improvements & Application

This project was originally built as an academic tool, but it holds potential for powerful future development. Here are three possible improvements or extensions :

1. **AI Model Input Generation**
By storing the analyzed data in a structured database, this application can serve as a valuable input source for machine learning models focused on financial prediction and stock behaviour analysis

2. **Strategy-Based Stock Recommendation**
Using the real-time calculated strategies (e.g., covered call, conversion and long straddle) and combining them with AI algorithms, the system can evolve into a recommendation engine that suggests the most profitable stocks to buy, tailored to user preferences or market trends.

3. **Automated Investment Platform**
An integrated mobile or web app could be developed to handle real-time stock purchases. With support for Iran's banking infrastructure and connection to the AI model mentioned above, the system could analyze, decide, and buy. This would complete a full cycle of smart, automated trading.

NOTE: The combination of these features could result in a next-generation investment assistant powered by real data, smart analysis, and automation.


## Authors

- [@Mohammadjavad Ganjtalab](https://github.com/mjavad-tech)

