﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNet.FileSystems;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Runtime;

namespace Microsoft.AspNet.Mvc.Razor
{
    public class RazorPreCompiler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IFileSystem _fileSystem;
        private readonly IMvcRazorHost _host;

        protected virtual string FileExtension
        {
            get
            {
                return ".cshtml";
            }
        }

        public RazorPreCompiler([NotNull] IServiceProvider designTimeServiceProvider) :
            this(designTimeServiceProvider, designTimeServiceProvider.GetService<IMvcRazorHost>())
        {
        }

        public RazorPreCompiler([NotNull] IServiceProvider designTimeServiceProvider,
                                [NotNull] IMvcRazorHost host)
        {
            _serviceProvider = designTimeServiceProvider;
            _host = host;

            var appEnv = _serviceProvider.GetService<IApplicationEnvironment>();
            _fileSystem = new PhysicalFileSystem(appEnv.ApplicationBasePath);
        }

        public virtual void CompileViews([NotNull] IBeforeCompileContext context)
        {
            var descriptors = CreateCompilationDescriptors(context);
            var collectionGenerator = new RazorFileInfoCollectionGenerator(
                                            descriptors,
                                            SyntaxTreeGenerator.GetParseOptions(context.CSharpCompilation));

            var tree = collectionGenerator.GenerateCollection();
            context.CSharpCompilation = context.CSharpCompilation.AddSyntaxTrees(tree);
        }

        protected virtual IReadOnlyList<RazorFileInfo> CreateCompilationDescriptors(
                                                            [NotNull] IBeforeCompileContext context)
        {
            var options = SyntaxTreeGenerator.GetParseOptions(context.CSharpCompilation);
            var list = new List<RazorFileInfo>();

            foreach (var info in GetFileInfosRecursive(string.Empty))
            {
                var descriptor = ParseView(info,
                                           context,
                                           options);

                if (descriptor != null)
                {
                    list.Add(descriptor);
                }
            }

            return list;
        }

        private IEnumerable<RelativeFileInfo> GetFileInfosRecursive(string currentPath)
        {
            IEnumerable<IFileInfo> fileInfos;
            string path = currentPath;

            if (!_fileSystem.TryGetDirectoryContents(path, out fileInfos))
            {
                yield break;
            }

            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.IsDirectory)
                {
                    var subPath = Path.Combine(path, fileInfo.Name);

                    foreach (var info in GetFileInfosRecursive(subPath))
                    {
                        yield return info;
                    }
                }
                else if (Path.GetExtension(fileInfo.Name)
                         .Equals(FileExtension, StringComparison.OrdinalIgnoreCase))
                {
                    var info = new RelativeFileInfo()
                    {
                        FileInfo = fileInfo,
                        RelativePath = Path.Combine(currentPath, fileInfo.Name),
                    };

                    yield return info;
                }
            }
        }

        protected virtual RazorFileInfo ParseView([NotNull] RelativeFileInfo fileInfo,
                                                  [NotNull] IBeforeCompileContext context,
                                                  [NotNull] CSharpParseOptions options)
        {
            using (var stream = fileInfo.FileInfo.CreateReadStream())
            {
                var results = _host.GenerateCode(fileInfo.RelativePath, stream);
                if (results.Success)
                {
                    var syntaxTree = SyntaxTreeGenerator.Generate(results.GeneratedCode, fileInfo.FileInfo.PhysicalPath, options);
                    var fullTypeName = results.GetMainClassName(_host, syntaxTree);

                    if (fullTypeName != null)
                    {
                        context.CSharpCompilation = context.CSharpCompilation.AddSyntaxTrees(syntaxTree);

                        var hash = RazorFileHash.GetHash(fileInfo.FileInfo);

                        return new RazorFileInfo()
                        {
                            FullTypeName = fullTypeName,
                            RelativePath = fileInfo.RelativePath,
                            LastModified = fileInfo.FileInfo.LastModified,
                            Length = fileInfo.FileInfo.Length,
                            Hash = hash,
                        };
                    }
                }
            }

            // TODO: Add diagnostics when view parsing/code generation failed.
            return null;
        }
    }
}

namespace Microsoft.Framework.Runtime
{
    [AssemblyNeutral]
    public interface IBeforeCompileContext
    {
        CSharpCompilation CSharpCompilation { get; set; }

        IList<ResourceDescription> Resources { get; }

        IList<Diagnostic> Diagnostics { get; }
    }
}
