﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using RichardSzalay.MockHttp.Matchers;
using RichardSzalay.MockHttp.Tests.Infrastructure;
using Xunit;

namespace RichardSzalay.MockHttp.Tests.Matchers
{
    public class FormDataMatcherTests
    {
        [Fact]
        public void Should_match_in_order()
        {
            bool result = Test(
                expected: "key1=value1&key2=value2",
                actual: "key1=value1&key2=value2"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_match_out_of_order()
        {
            bool result = Test(
                expected: "key2=value2&key1=value1",
                actual: "key1=value1&key2=value2"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_match_multiple_values()
        {
            bool result = Test(
                expected: "key1=value1&key1=value2",
                actual: "key1=value2&key1=value1"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_support_matching_empty_values()
        {
            bool result = Test(
                expected: "key2=value2&key1",
                actual: "key1&key2=value2"
                );

            Assert.True(result);
        }

        [Fact]
        public void Should_fail_for_incorrect_values()
        {
            bool result = Test(
                expected: "key1=value1&key2=value3",
                actual: "key1=value1&key2=value2"
                );

            Assert.False(result);
        }

        [Fact]
        public void Should_fail_for_missing_keys()
        {
            bool result = Test(
                expected: "key2=value2&key1=value1",
                actual: "key1=value1&key3=value3"
                );

            Assert.False(result);
        }

        [Fact]
        public void Should_not_fail_for_additional_keys()
        {
            bool result = Test(
                expected: "key1=value1&key2=value2",
                actual: "key1=value1&key2=value2&key3=value3"
                );

            Assert.True(result);
        }

        private bool Test(string expected, string actual)
        {
            var sut = new FormDataMatcher(expected);

            FormUrlEncodedContent content = new FormUrlEncodedContent(
                HttpHelpers.ParseQueryString(actual)
                );

            return sut.Matches(new HttpRequestMessage(HttpMethod.Get,
                "http://tempuri.org/home") { Content = content });
        }

        [Fact]
        public void Should_fail_for_non_form_data()
        {
            var content = new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key=value"));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            var result = Test(
                expected: "key=value",
                actual: content
                );

            Assert.False(result);
        }

        [Fact]
        public void Supports_multipart_formdata_content()
        {
            var content = new MultipartFormDataContent
            {
                new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key=value"))
            };

            var result = Test(
                expected: "key=value", 
                actual: content
                );

            Assert.True(result);
        }

        [Fact]
        public void Matches_form_data_across_multipart_entries()
        {
            var content = new MultipartFormDataContent
            {
                new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key1=value1")),
                new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key2=value2"))
            };

            var result = Test(
                expected: "key1=value1&key2=value2",
                actual: content
                );

            Assert.True(result);
        }

        [Fact]
        public void Does_not_match_form_data_on_non_form_data_multipart_entries()
        {
            var content = new MultipartFormDataContent
            {
                new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key1=value1")),
                new FormUrlEncodedContent(HttpHelpers.ParseQueryString("key2=value2"))
            };

            content.First().Headers.ContentType = new MediaTypeHeaderValue("text/plain");

            var result = Test(
                expected: "key1=value1&key2=value2",
                actual: content
                );

            Assert.False(result);
        }

        private bool Test(string expected, HttpContent actual)
        {
            var sut = new FormDataMatcher(expected);

            return sut.Matches(new HttpRequestMessage(HttpMethod.Get,
                "http://tempuri.org/home") { Content = actual });
        }
    }
}
