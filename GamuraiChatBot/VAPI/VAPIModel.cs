using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GamuraiChatBot.VAPI
{
    #region Generic Response Classes
    public class VAPIGenericResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
    }
    #endregion

    #region VeonChatBot
    #region Get Hairstylist/Beautician Names
    public class VAPIGenericBranchInfoModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
    }

    public class VAPIStylistNamesResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public List<string> Data { get; set; }
    }

    public class VAPIStylistInfoResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public List<StylistInfoModel> Data { get; set; }
    }

    public class StylistInfoModel
    {
        public string StylistName { get; set; }
        public string StylistPicSrc { get; set; }
    }
    #endregion

    #region Service Enquiry
    public class VAPIServiceEnquiryModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string ServiceName { get; set; }
    }

    public class VAPIServiceResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public List<VAPIServiceResponseModel> Data { get; set; }
    }

    public class VAPIServiceResponseModel
    {
        public string ServiceName { get; set; }
        public double ServicePrice { get; set; }
    }
    #endregion

    #region Product Enquiry
    public class VAPIProductEnquiryModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string ProductName { get; set; }
    }

    public class VAPIProductResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public List<VAPIProductResponseModel> Data { get; set; }
    }

    public class VAPIProductResponseModel
    {
        public string ProductName { get; set; }
        public double ProductPrice { get; set; }
    }
    #endregion

    #region Hairstylist Availability
    public class VAPIHairstylistAvailabilityEnquiryModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string HairstylistName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }

    public class VAPIHairstylistAvailabilityResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public VAPIHairstylistAvailabilityResponseModel Data { get; set; }
    }

    public class VAPIHairstylistAvailabilityResponseModel
    {
        public string HairstylistId { get; set; }
        public string HairstylistName { get; set; }
        public string Date { get; set; }
        public List<string> AvailableTimeSlot { get; set; }
    }
    #endregion

    #region Book Appointment
    public class VAPIHairstylistAppointmentModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string HairstylistName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
    }

    public class VAPIHairstylistAppointmentResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public List<VAPIHairstylistAppointmentResponseModel> Data { get; set; }
    }

    public class VAPIHairstylistAppointmentResponseModel
    {
        public string HairstylistId { get; set; }
        public string HairstylistName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
    #endregion

    #region Update Booking
    public class VAPIUpdateBookingModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string HairstylistName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
    }

    public class VAPIUpdateBookingResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public VAPIUpdateBookingResponseModel Data { get; set; }
    }

    public class VAPIUpdateBookingResponseModel
    {
        public string HairstylistId { get; set; }
        public string HairstylistName { get; set; }
        public string Date { get; set; }
        public string Time { get; set; }
    }
    #endregion

    #region Check Booking
    public class VAPICheckBookingModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
    }

    public class VAPICheckBookingResponse
    {
        public string Status { get; set; }
        public string Msg { get; set; }
        public List<VAPICheckBookingResponseModel> Data { get; set; }
    }

    public class VAPICheckBookingResponseModel
    {
        public DateTime DateTime { get; set; }
        public string HairstylistName { get; set; }
    }
    #endregion

    #region Delete Booking
    public class VAPIDeleteBookingModel
    {
        public string AppId { get; set; }
        public string BranchId { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string DateTime { get; set; }
    }
    #endregion
    #endregion
}