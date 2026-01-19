using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamuraiChatBot
{
    public static class StaticEnum
    {
        public partial class Flow
        {

            public const string previousIntent = "previousIntent";
            public const string frustrationLevel = "frustrationLevel";


        }


        public partial class Intents
        {

            public  const  string CheckServicePrice = "CheckServicePrice";
            public const string CancelBooking = "CancelBooking";
            public const string CheckPaymentMethod = "CheckPaymentMethod";
            public const string CheckPromotion = "CheckPromotion";
            public const string UpdateBooking = "UpdateBooking";
            public const string CheckContactInfo = "CheckContactInfo";
            public const string CheckProductPrice = "CheckProductPrice";
            public const string CheckBooking = "CheckBooking";
            public const string MakeBooking = "MakeBooking";
            public const string None = "None";
            public const string CheckStaffNames = "CheckStaffNames";



        }

        public partial class Entities
        {
            public static readonly string number = "builtin.number";
            public static readonly string datetime = "builtin.datetime";
            public static readonly string date = "builtin.datetime.date";
            public static readonly string time = "builtin.datetime.time";

            public static readonly string Services = "Services";
            public static readonly string HairstylistServices = "Services::HairstylistServices";
            public static readonly string BeauticianServices = "Services::BeauticianServices";
                                                  
            public static readonly string Products = "Products";
            public static readonly string HairProduct = "Products::HairProduct";
            public static readonly string BeautyProduct = "Products::BeautyProduct";

            public static readonly string ContactInfo = "ContactInfo";
            public static readonly string Address = "ContactInfo::address";
            public static readonly string ContactNumber = "ContactInfo::contact number";
            public static readonly string Email = "ContactInfo::email";
            public static readonly string OperatingHours = "ContactInfo::operating hours";

            public static readonly string PaymentMethods = "PaymentMethods";

            public static readonly string Staff = "Staff";
            public static readonly string Hairstylist = "Staff::Hairstylist";
            public static readonly string Beautician = "Staff::Beautician";

            public static readonly string CustomerType = "CustomerType";
            public static readonly string Concession = "CustomerType::Concession";
            public static readonly string Member = "CustomerType::Member";

            public static readonly string DiscountType = "DiscountType";
            public static readonly string Permanent = "DiscountType::Permanent";
            public static readonly string Temporary = "DiscountType::Temporary";


            public static readonly string Booking = "Booking";
        }

        public partial class pendingIntent
        {

            public static readonly string toCheckProductPrice = "pendingCheckProductPrice";
            public static readonly string toCheckContactInfo = "pendingCheckContactInfo";
            public static readonly string toCheckServicePrice = "pendingCheckServicePrice";
            public static readonly string toMakeBooking = "pendingMakeBooking";
            public static readonly string toCheckBooking = "pendingCheckBooking";
            public static readonly string toUpdateBooking = "pendingUpdateBooking";
            public static readonly string toCancelBooking = "pendingCancelBooking";
            public static readonly string toCheckPaymentMethod = "pendingCheckPaymentMethod";


        }

        public partial class pendingObject
        {
            public static readonly string CheckStaffName = "currentCheckStaffName";
            public static readonly string CheckProductPrice = "currentCheckProductPrice";
            public static readonly string CheckContactInfo = "currentCheckContactInfo";
            public static readonly string CheckServicePrice = "currentCheckServicePrice";
            public static readonly string MakeBooking = "currentMakeBooking";
            public static readonly string CheckBooking = "currentCheckBooking";
            public static readonly string UpdateBooking = "currentUpdateBooking";
            public static readonly string CancelBooking = "currentCancelBooking";
            public static readonly string CheckPaymentMethod = "currentCheckPaymentMethod";

        }

        public partial class StandardBotResponse
        {

            public static readonly string Greetings = @"Hi there! I'm a chatbot and I'm here to assist you
                                        I'm able to perform some functions to help you today.
                                        If you need help, just type ""help""
                                        Some feature which I perform are 
                                        1. Checking, Making, Cancelling and Updating of your Booking 
                                        2. Checking of services prices
                                        3. Checking of product prices
                                        4. Enquires on operating hours and address
                                        5. Enquires on type of payment method accepted
                                        ";
            public static readonly string Help = @"Some feature which I perform are 
                                        1. Checking, Making, Cancelling and Updating of your Booking 
                                        2. Checking of services prices
                                        3. Checking of product prices
                                        4. Enquires on operating hours and address
                                        5. Enquires on type of payment method accepted
                                        ";

        }
    }
}