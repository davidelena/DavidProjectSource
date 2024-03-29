﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DavidProject
{
    public class RequestHandler
    {
        private string partialProtocal = string.Empty;

        public string[] GetActualString(string input)
        {
            return GetActualString(input, null);
        }

        public string[] GetActualString(string input, List<string> outputList)
        {
            if (outputList == null)
                outputList = new List<string>();

            if (!string.IsNullOrEmpty(partialProtocal))
                input = partialProtocal + input;

            string output = string.Empty;
            string pattern = @"(?<=^\[length=)(\d+)(?=\])";
            int length;

            if (Regex.IsMatch(input, pattern))
            {
                Match m = Regex.Match(input, pattern);
                length = Convert.ToInt32(m.Groups[0].Value);

                int startIndex = input.IndexOf("]") + 1;
                output = input.Substring(startIndex);

                if (output.Length == length)
                {
                    // 如果output的长度与消息字符串的应有长度相等
                    // 说明刚好是完整的一条信息
                    outputList.Add(output);
                    partialProtocal = string.Empty;
                }
                else if (output.Length < length)
                {
                    // 如果之后的长度小于应有的长度，
                    // 说明没有发完整，则应将整条信息，包括元数据，全部缓存
                    // 与下一条数据合并起来再进行处理
                    partialProtocal = input;
                    // 此时程序应该退出，因为需要等待下一条数据到来才能继续处理
                }
                else if (output.Length > length)
                {
                    // 如果之后的长度大于应有的长度，
                    // 说明消息发完整了，但是有多余的数据
                    // 多余的数据可能是截断消息，也可能是多条完整消息
                    output = output.Substring(0, length);
                    outputList.Add(output);
                    partialProtocal = string.Empty;

                    input = input.Substring(startIndex + length);

                    GetActualString(input, outputList);
                }
            }
            else
            {
                partialProtocal = input;
            }

            return outputList.ToArray();
        }

        public static void Test()
        {
            RequestHandler handler = new RequestHandler();
            string input;

            //第一种情况测试，一条完整消息发送
            input = "[length=13]明天中秋，祝大家节日快乐！";
            handler.PrintOutput(input);

            //第二种情况测试，两条完整消息一次发送
            input = "明天中秋，祝大家节日快乐！";
            input = string.Format("[length=13]{0}[length=13]{0}", input);
            handler.PrintOutput(input);

            //第三种情况测试，两条条不完整消息发送A
            input = "[length=13]明天中秋，祝大家节日快乐！[length=13]明天中秋，";
            handler.PrintOutput(input);

            input = "祝大家节日快乐！";
            handler.PrintOutput(input);

            //第四种情况测试，两条条不完整消息发送B
            input = "[length=13]明天中秋，";
            handler.PrintOutput(input);

            input = "祝大家节日快乐！[length=13]明天中秋，祝大家节日快乐！";
            handler.PrintOutput(input);

            //第五种情况，元数据不完整即自定义协议头不完整
            input = "[leng";
            handler.PrintOutput(input); //不输出

            input = "th=13]明天中秋，祝大家节日快乐！";
            handler.PrintOutput(input);
        }


        // 用于测试输出
        private void PrintOutput(string input)
        {
            Console.WriteLine(input);
            string[] outputArray = GetActualString(input);
            foreach (string output in outputArray)
            {
                Console.WriteLine(output);
            }
            Console.WriteLine();
        }
    }
}
