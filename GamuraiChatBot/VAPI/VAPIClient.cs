using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;

namespace GamuraiChatBot.VAPI
{
    public class VAPIClient
    {
        protected string baseUrl;
        protected Dictionary<string, string> dataToSend = new Dictionary<string, string>();

        public VAPIClient(string url, Dictionary<string, string> data)
        {
            this.baseUrl = url;
            this.dataToSend = data;
        }

        public VAPIGenericResponse VAPIPing()
        {
            try
            {
                VAPIGenericResponse response = new VAPIGenericResponse();
                var client = new HttpClient();
                baseUrl = "https://veon3d.azurewebsites.net/VAPI/Ping";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";
                httpWebRequest.ContentLength = 0;
                var httpResponse = httpWebRequest.GetResponseAsync().Result;

                String str = "";
                using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    str = reader.ReadToEnd();
                    response = (VAPIGenericResponse)JsonConvert.DeserializeObject(str, typeof(VAPIGenericResponse));
                    return response;
                }
            }
            catch (Exception ex)
            {
                VAPIGenericResponse response = new VAPIGenericResponse();
                response.Status = "Failed";
                response.Msg = ex.Message;
                return response;
            }
        }

        public VAPIStylistNamesResponse GetStylistNames()
        {
            VAPIStylistNamesResponse response = new VAPIStylistNamesResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/GetStylistNames";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIStylistNamesResponse)JsonConvert.DeserializeObject(str, typeof(VAPIStylistNamesResponse));
                return response;
            }
        }

        public VAPIStylistNamesResponse GetBeauticianNames()
        {
            VAPIStylistNamesResponse response = new VAPIStylistNamesResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/GetBeauticianNames";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIStylistNamesResponse)JsonConvert.DeserializeObject(str, typeof(VAPIStylistNamesResponse));
                return response;
            }
        }

        public VAPIStylistInfoResponse GetStylistInfo()
        {
            VAPIStylistInfoResponse response = new VAPIStylistInfoResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/GetStylistInfo";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIStylistInfoResponse)JsonConvert.DeserializeObject(str, typeof(VAPIStylistInfoResponse));
                return response;
            }
        }

        public VAPIServiceResponse CheckServicePrice()
        {
            VAPIServiceResponse response = new VAPIServiceResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/CheckServicePrice";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIServiceResponse)JsonConvert.DeserializeObject(str, typeof(VAPIServiceResponse));
                return response;
            }
        }

        public VAPIProductResponse CheckProductPrice()
        {
            VAPIProductResponse response = new VAPIProductResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/CheckProductPrice";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIProductResponse)JsonConvert.DeserializeObject(str, typeof(VAPIProductResponse));
                return response;
            }
        }

        //to do chun siong call this API to find free dates
        /// <summary>
        /// enter datetime in string format, name in string format
        /// </summary>
        /// <returns></returns>
        public VAPIHairstylistAvailabilityResponse CheckHairstylistAvailability()
        {
            VAPIHairstylistAvailabilityResponse response = new VAPIHairstylistAvailabilityResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/CheckHairstylistAvailability";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIHairstylistAvailabilityResponse)JsonConvert.DeserializeObject(str, typeof(VAPIHairstylistAvailabilityResponse));
                return response;
            }
        }

        public VAPIHairstylistAppointmentResponse BookAppointment()
        {
            VAPIHairstylistAppointmentResponse response = new VAPIHairstylistAppointmentResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/BookAppointment";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIHairstylistAppointmentResponse)JsonConvert.DeserializeObject(str, typeof(VAPIHairstylistAppointmentResponse));
                return response;
            }
        }

        public VAPIUpdateBookingResponse UpdateBooking()
        {
            VAPIUpdateBookingResponse response = new VAPIUpdateBookingResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/UpdateBooking";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIUpdateBookingResponse)JsonConvert.DeserializeObject(str, typeof(VAPIUpdateBookingResponse));
                return response;
            }
        }

        public VAPICheckBookingResponse CheckBooking()
        {
            VAPICheckBookingResponse response = new VAPICheckBookingResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/CheckBooking";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPICheckBookingResponse)JsonConvert.DeserializeObject(str, typeof(VAPICheckBookingResponse));
                return response;
            }
        }

        public VAPIGenericResponse DeleteBooking()
        {
            VAPIGenericResponse response = new VAPIGenericResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/DeleteBooking";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIGenericResponse)JsonConvert.DeserializeObject(str, typeof(VAPIGenericResponse));
                return response;
            }
        }

        public VAPIStylistInfoResponse GetPaymentMethodAvailable()
        {
            VAPIStylistInfoResponse response = new VAPIStylistInfoResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/GetPaymentMethodAvailable";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIStylistInfoResponse)JsonConvert.DeserializeObject(str, typeof(VAPIStylistInfoResponse));
                return response;
            }
        }

        public VAPIGenericResponse GetHumanAttention()
        {
            VAPIGenericResponse response = new VAPIGenericResponse();
            var client = new HttpClient();
            baseUrl = "https://veon3d.azurewebsites.net/VAPI/GetHumanAttention";
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(baseUrl);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            var stream = httpWebRequest.GetRequestStreamAsync().Result;
            using (var streamWriter = new StreamWriter(stream))
            {
                string oString = JsonConvert.SerializeObject(dataToSend);
                streamWriter.Write(oString);
                streamWriter.Flush();
            }

            var httpResponse = httpWebRequest.GetResponseAsync().Result;

            String str = "";
            using (StreamReader reader = new StreamReader(httpResponse.GetResponseStream()))
            {
                str = reader.ReadToEnd();
                response = (VAPIGenericResponse)JsonConvert.DeserializeObject(str, typeof(VAPIGenericResponse));
                return response;
            }
        }
    }
}