﻿using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MiniCover.HitServices
{
    public static class HitService
    {
        public static MethodContext EnterMethod(string fileName)
        {
            return new MethodContext(fileName);
        }

        public class MethodContext
        {
            private readonly string filePath;
            private static readonly AsyncLocal<HitTestMethod> TestMethodCache = new AsyncLocal<HitTestMethod>();
            private static Dictionary<string, Stream> filesStream = new Dictionary<string, Stream>();

            private readonly HitTestMethod testMethod;
            private readonly bool clearTestMethodCache;

            public MethodContext(string filePath)
            {
                this.filePath = filePath;
                if (TestMethodCache.Value == null)
                {
                    TestMethodCache.Value = HitTestMethod.From(TestMethodUtils.GetTestMethod());
                    clearTestMethodCache = true;
                }

                testMethod = TestMethodCache.Value;
            }

            public void HitInstruction(int id)
            {
                testMethod.HasHit(id);

            }

            public void Exit()
            {
                if (clearTestMethodCache)
                {
                    Save();
                    TestMethodCache.Value = null;
                }
            }

            private void Save()
            {
                var fileStream = GetFileStream();
                lock (fileStream)
                {
                    testMethod.Serialize(fileStream);
                    fileStream.Flush();
                }
            }

            private Stream GetFileStream()
            {
                lock (filesStream)
                {
                    if (!filesStream.ContainsKey(filePath))
                        filesStream[filePath] = File.Open(this.filePath, FileMode.Append, FileAccess.Write, FileShare.None);
                }

                return filesStream[filePath];
            }
        }
    }
}