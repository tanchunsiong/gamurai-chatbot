# GamuraiChatbot

Conversational AI chatbot for salon booking services using Microsoft Bot Framework and LUIS natural language understanding.

## Overview

Production-ready chatbot built for Jawed Habib salon services. Handles appointment bookings, service inquiries, pricing information, and customer service through natural language conversations.

## Features

- Natural language understanding with LUIS
- Booking management (create, check, cancel)
- Service and product price inquiries
- Payment method information
- Multi-turn conversation flows
- Entity extraction and validation

## Technology Stack

**Backend:** C#, ASP.NET MVC 5  
**Framework:** Microsoft Bot Framework 3.2  
**NLU:** LUIS (Language Understanding Intelligent Service)  
**Database:** SQL Server

## Project Structure

```
GamuraiChatbot/
├── Controllers/
│   ├── MessagesController.cs
│   └── HomeController.cs
├── Entities/
│   ├── Booking.cs
│   ├── Service.cs
│   └── Product.cs
├── HelperClasses/
├── LUIS/
│   └── Veon ChatBot For Jawed Habib.json
└── Web.config
```

## LUIS Intents

- MakeBooking
- CheckBooking
- CancelBooking
- CheckServicePrice
- CheckProductPrice
- PaymentMethods
- Greeting
- Help

## Installation

1. Clone repository
```bash
git clone https://github.com/tanchunsiong/gamurai-chatbot.git
```

2. Open solution in Visual Studio

3. Restore NuGet packages

4. **Configure credentials in Web.config:**
   - Copy `Web.config.example` to `Web.config`
   - Add your Bot Framework App ID and App Secret
   - Add your LUIS App ID and Subscription Key
   - **Never commit Web.config with real credentials!**

5. Import LUIS model from `/LUIS` folder

6. Build and deploy

## Usage

Interact with bot through:
- Web interface
- Microsoft Teams
- Skype
- Other Bot Framework channels

## License

MIT License

## Links

- Blog post: [Building a Salon Booking Chatbot with LUIS and Bot Framework](https://www.tanchunsiong.com/2019/03/building-a-salon-booking-chatbot-with-luis-and-bot-framework/)
- GitHub: [@tanchunsiong](https://github.com/tanchunsiong)
- LinkedIn: [tanchunsiong](https://www.linkedin.com/in/tanchunsiong)
- X/Twitter: [@tanchunsiong](https://x.com/tanchunsiong)

---

*Project created March 2019*
