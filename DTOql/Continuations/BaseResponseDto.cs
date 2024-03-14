using DTOql.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace DTOql.Continuations
{
    public interface IBaseResponseDto<T>
    {
        bool IsSuccess { get; }
        int ReturnCode { get; set; }
        PagingWithSortModel QueryOptions { get; set; }
        T Data { get; set; }
        string Message { get; set; }

        string TransactionId { get; set; }

        List<string> Errors { get; }
        string RequestCorrelationId { get; set; }


        Dictionary<string, List<string>> PropErrors { get; }
    }
    public class BaseResponseDto<T> : IBaseResponseDto<T>
    {
        [JsonProperty]
        public bool IsSuccess { get; private set; }
        public int ReturnCode { get; set; }
        public PagingWithSortModel QueryOptions { get; set; }
        [JsonProperty]
        public T Data { get; set; }
        public string Message { get; set; }

        public string TransactionId { get; set; }
        public string RequestCorrelationId { get; set; }
        [JsonProperty]
        public List<string> Errors { get; private set; }
        [JsonProperty]
        public Dictionary<string, List<string>> PropErrors { get; private set; }




        public static explicit operator BaseResponseDto<object>(BaseResponseDto<T> response)
        {
            return new BaseResponseDto<object>
            {
                Data = response.Data,
                Errors = response.Errors,
                IsSuccess = response.IsSuccess,
                Message = response.Message,
                PropErrors = response.PropErrors,
                QueryOptions = response.QueryOptions,
                ReturnCode = response.ReturnCode,
                TransactionId = response.TransactionId
            };
        }
        public BaseResponseDto()
        {
            SetSuccess(default, 0);
        }
        private void SetSuccess(T data, int returnCode, PagingWithSortModel queryOptions = default, string message = default)
        {
            IsSuccess = true;
            this.ReturnCode = returnCode;
            this.QueryOptions = queryOptions;
            this.Message = message;
            this.Data = data;
            TransactionId = default;
            PropErrors = default;
        }

        public static BaseResponseDto<T> InternalSuccess(T data, PagingWithSortModel queryOptions = default, string message = default)
        {
            var response = new BaseResponseDto<T>();
            response.SetSuccess(data, 0, queryOptions, message);
            return response;
        }

        public static BaseResponseDto<T> Success(T data, PagingWithSortModel queryOptions = default, string message = default)
        {
            var response = new BaseResponseDto<T>();
            response.SetSuccess(data, 0, queryOptions, message);
            return response;
        }

        public static BaseResponseDto<T> InternalError(string message, string transactionId = default, List<string> errors = null, Dictionary<string, List<string>> propErrors = null)
        {
            var response = new BaseResponseDto<T>();
            response.SetError(message, 500);
            return response;
        }


        private void SetError(string message, int returnCode, string transactionId = default, List<string> errors = null, Dictionary<string, List<string>> propErrors = null)
        {
            IsSuccess = false;
            this.Message = message;
            this.ReturnCode = returnCode;
            this.TransactionId = transactionId;
            this.Errors = errors;
            this.PropErrors = propErrors;
            Data = default;
        }

        public BaseResponseDto<T> Error<TParameter>(BaseResponseDto<TParameter> fromErrorResponse)
        {
            this.SetError(fromErrorResponse.Message, fromErrorResponse.ReturnCode, fromErrorResponse.TransactionId, fromErrorResponse.Errors, fromErrorResponse.PropErrors);

            return this;
        }

        public BaseResponseDto<T> Error(string message, int returnCode)
        {
            this.SetError(message, returnCode, null, new List<string>() { message });

            return this;
        }

        public BaseResponseDto<T> Success(T data, int returnCode)
        {
            this.SetSuccess(data, returnCode);
            return this;
        }

    }




}
