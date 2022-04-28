// <auto-generated>
//     Generated by the protocol buffer compiler.  DO NOT EDIT!
//     source: solar_mapping_and_relocalization_proxy.proto
// </auto-generated>
// Original file comments:
// Copyright (c) 2021 All Right Reserved, B-com http://www.b-com.com/
//
// This file is subject to the B<>Com License.
// All other rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
// KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
#pragma warning disable 0414, 1591
#region Designer generated code

using grpc = global::Grpc.Core;

namespace Com.Bcom.Solar.Gprc {
  public static partial class SolARMappingAndRelocalizationProxy
  {
    static readonly string __ServiceName = "com.bcom.solar.gprc.SolARMappingAndRelocalizationProxy";

    static void __Helper_SerializeMessage(global::Google.Protobuf.IMessage message, grpc::SerializationContext context)
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (message is global::Google.Protobuf.IBufferMessage)
      {
        context.SetPayloadLength(message.CalculateSize());
        global::Google.Protobuf.MessageExtensions.WriteTo(message, context.GetBufferWriter());
        context.Complete();
        return;
      }
      #endif
      context.Complete(global::Google.Protobuf.MessageExtensions.ToByteArray(message));
    }

    static class __Helper_MessageCache<T>
    {
      public static readonly bool IsBufferMessage = global::System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(global::Google.Protobuf.IBufferMessage)).IsAssignableFrom(typeof(T));
    }

    static T __Helper_DeserializeMessage<T>(grpc::DeserializationContext context, global::Google.Protobuf.MessageParser<T> parser) where T : global::Google.Protobuf.IMessage<T>
    {
      #if !GRPC_DISABLE_PROTOBUF_BUFFER_SERIALIZATION
      if (__Helper_MessageCache<T>.IsBufferMessage)
      {
        return parser.ParseFrom(context.PayloadAsReadOnlySequence());
      }
      #endif
      return parser.ParseFrom(context.PayloadAsNewBuffer());
    }

    static readonly grpc::Marshaller<global::Com.Bcom.Solar.Gprc.PipelineModeValue> __Marshaller_com_bcom_solar_gprc_PipelineModeValue = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Com.Bcom.Solar.Gprc.PipelineModeValue.Parser));
    static readonly grpc::Marshaller<global::Com.Bcom.Solar.Gprc.Empty> __Marshaller_com_bcom_solar_gprc_Empty = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Com.Bcom.Solar.Gprc.Empty.Parser));
    static readonly grpc::Marshaller<global::Com.Bcom.Solar.Gprc.CameraParameters> __Marshaller_com_bcom_solar_gprc_CameraParameters = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Com.Bcom.Solar.Gprc.CameraParameters.Parser));
    static readonly grpc::Marshaller<global::Com.Bcom.Solar.Gprc.Frame> __Marshaller_com_bcom_solar_gprc_Frame = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Com.Bcom.Solar.Gprc.Frame.Parser));
    static readonly grpc::Marshaller<global::Com.Bcom.Solar.Gprc.RelocalizationResult> __Marshaller_com_bcom_solar_gprc_RelocalizationResult = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Com.Bcom.Solar.Gprc.RelocalizationResult.Parser));
    static readonly grpc::Marshaller<global::Com.Bcom.Solar.Gprc.Message> __Marshaller_com_bcom_solar_gprc_Message = grpc::Marshallers.Create(__Helper_SerializeMessage, context => __Helper_DeserializeMessage(context, global::Com.Bcom.Solar.Gprc.Message.Parser));

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.PipelineModeValue, global::Com.Bcom.Solar.Gprc.Empty> __Method_Init = new grpc::Method<global::Com.Bcom.Solar.Gprc.PipelineModeValue, global::Com.Bcom.Solar.Gprc.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Init",
        __Marshaller_com_bcom_solar_gprc_PipelineModeValue,
        __Marshaller_com_bcom_solar_gprc_Empty);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty> __Method_Start = new grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Start",
        __Marshaller_com_bcom_solar_gprc_Empty,
        __Marshaller_com_bcom_solar_gprc_Empty);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty> __Method_Stop = new grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Stop",
        __Marshaller_com_bcom_solar_gprc_Empty,
        __Marshaller_com_bcom_solar_gprc_Empty);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.CameraParameters, global::Com.Bcom.Solar.Gprc.Empty> __Method_SetCameraParameters = new grpc::Method<global::Com.Bcom.Solar.Gprc.CameraParameters, global::Com.Bcom.Solar.Gprc.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SetCameraParameters",
        __Marshaller_com_bcom_solar_gprc_CameraParameters,
        __Marshaller_com_bcom_solar_gprc_Empty);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.Frame, global::Com.Bcom.Solar.Gprc.RelocalizationResult> __Method_RelocalizeAndMap = new grpc::Method<global::Com.Bcom.Solar.Gprc.Frame, global::Com.Bcom.Solar.Gprc.RelocalizationResult>(
        grpc::MethodType.Unary,
        __ServiceName,
        "RelocalizeAndMap",
        __Marshaller_com_bcom_solar_gprc_Frame,
        __Marshaller_com_bcom_solar_gprc_RelocalizationResult);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.RelocalizationResult> __Method_Get3DTransform = new grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.RelocalizationResult>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Get3DTransform",
        __Marshaller_com_bcom_solar_gprc_Empty,
        __Marshaller_com_bcom_solar_gprc_RelocalizationResult);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty> __Method_Reset = new grpc::Method<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "Reset",
        __Marshaller_com_bcom_solar_gprc_Empty,
        __Marshaller_com_bcom_solar_gprc_Empty);

    static readonly grpc::Method<global::Com.Bcom.Solar.Gprc.Message, global::Com.Bcom.Solar.Gprc.Empty> __Method_SendMessage = new grpc::Method<global::Com.Bcom.Solar.Gprc.Message, global::Com.Bcom.Solar.Gprc.Empty>(
        grpc::MethodType.Unary,
        __ServiceName,
        "SendMessage",
        __Marshaller_com_bcom_solar_gprc_Message,
        __Marshaller_com_bcom_solar_gprc_Empty);

    /// <summary>Service descriptor</summary>
    public static global::Google.Protobuf.Reflection.ServiceDescriptor Descriptor
    {
      get { return global::Com.Bcom.Solar.Gprc.SolarMappingAndRelocalizationProxyReflection.Descriptor.Services[0]; }
    }

    /// <summary>Base class for server-side implementations of SolARMappingAndRelocalizationProxy</summary>
    [grpc::BindServiceMethod(typeof(SolARMappingAndRelocalizationProxy), "BindService")]
    public abstract partial class SolARMappingAndRelocalizationProxyBase
    {
      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.Empty> Init(global::Com.Bcom.Solar.Gprc.PipelineModeValue request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.Empty> Start(global::Com.Bcom.Solar.Gprc.Empty request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.Empty> Stop(global::Com.Bcom.Solar.Gprc.Empty request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.Empty> SetCameraParameters(global::Com.Bcom.Solar.Gprc.CameraParameters request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.RelocalizationResult> RelocalizeAndMap(global::Com.Bcom.Solar.Gprc.Frame request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.RelocalizationResult> Get3DTransform(global::Com.Bcom.Solar.Gprc.Empty request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.Empty> Reset(global::Com.Bcom.Solar.Gprc.Empty request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

      public virtual global::System.Threading.Tasks.Task<global::Com.Bcom.Solar.Gprc.Empty> SendMessage(global::Com.Bcom.Solar.Gprc.Message request, grpc::ServerCallContext context)
      {
        throw new grpc::RpcException(new grpc::Status(grpc::StatusCode.Unimplemented, ""));
      }

    }

    /// <summary>Client for SolARMappingAndRelocalizationProxy</summary>
    public partial class SolARMappingAndRelocalizationProxyClient : grpc::ClientBase<SolARMappingAndRelocalizationProxyClient>
    {
      /// <summary>Creates a new client for SolARMappingAndRelocalizationProxy</summary>
      /// <param name="channel">The channel to use to make remote calls.</param>
      public SolARMappingAndRelocalizationProxyClient(grpc::ChannelBase channel) : base(channel)
      {
      }
      /// <summary>Creates a new client for SolARMappingAndRelocalizationProxy that uses a custom <c>CallInvoker</c>.</summary>
      /// <param name="callInvoker">The callInvoker to use to make remote calls.</param>
      public SolARMappingAndRelocalizationProxyClient(grpc::CallInvoker callInvoker) : base(callInvoker)
      {
      }
      /// <summary>Protected parameterless constructor to allow creation of test doubles.</summary>
      protected SolARMappingAndRelocalizationProxyClient() : base()
      {
      }
      /// <summary>Protected constructor to allow creation of configured clients.</summary>
      /// <param name="configuration">The client configuration.</param>
      protected SolARMappingAndRelocalizationProxyClient(ClientBaseConfiguration configuration) : base(configuration)
      {
      }

      public virtual global::Com.Bcom.Solar.Gprc.Empty Init(global::Com.Bcom.Solar.Gprc.PipelineModeValue request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Init(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Init(global::Com.Bcom.Solar.Gprc.PipelineModeValue request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Init, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> InitAsync(global::Com.Bcom.Solar.Gprc.PipelineModeValue request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return InitAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> InitAsync(global::Com.Bcom.Solar.Gprc.PipelineModeValue request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Init, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Start(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Start(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Start(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Start, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> StartAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StartAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> StartAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Start, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Stop(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Stop(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Stop(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Stop, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> StopAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return StopAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> StopAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Stop, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty SetCameraParameters(global::Com.Bcom.Solar.Gprc.CameraParameters request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SetCameraParameters(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty SetCameraParameters(global::Com.Bcom.Solar.Gprc.CameraParameters request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SetCameraParameters, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> SetCameraParametersAsync(global::Com.Bcom.Solar.Gprc.CameraParameters request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SetCameraParametersAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> SetCameraParametersAsync(global::Com.Bcom.Solar.Gprc.CameraParameters request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SetCameraParameters, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.RelocalizationResult RelocalizeAndMap(global::Com.Bcom.Solar.Gprc.Frame request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RelocalizeAndMap(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.RelocalizationResult RelocalizeAndMap(global::Com.Bcom.Solar.Gprc.Frame request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_RelocalizeAndMap, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.RelocalizationResult> RelocalizeAndMapAsync(global::Com.Bcom.Solar.Gprc.Frame request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return RelocalizeAndMapAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.RelocalizationResult> RelocalizeAndMapAsync(global::Com.Bcom.Solar.Gprc.Frame request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_RelocalizeAndMap, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.RelocalizationResult Get3DTransform(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Get3DTransform(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.RelocalizationResult Get3DTransform(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Get3DTransform, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.RelocalizationResult> Get3DTransformAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Get3DTransformAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.RelocalizationResult> Get3DTransformAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Get3DTransform, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Reset(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return Reset(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty Reset(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_Reset, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> ResetAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return ResetAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> ResetAsync(global::Com.Bcom.Solar.Gprc.Empty request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_Reset, null, options, request);
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty SendMessage(global::Com.Bcom.Solar.Gprc.Message request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SendMessage(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual global::Com.Bcom.Solar.Gprc.Empty SendMessage(global::Com.Bcom.Solar.Gprc.Message request, grpc::CallOptions options)
      {
        return CallInvoker.BlockingUnaryCall(__Method_SendMessage, null, options, request);
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> SendMessageAsync(global::Com.Bcom.Solar.Gprc.Message request, grpc::Metadata headers = null, global::System.DateTime? deadline = null, global::System.Threading.CancellationToken cancellationToken = default(global::System.Threading.CancellationToken))
      {
        return SendMessageAsync(request, new grpc::CallOptions(headers, deadline, cancellationToken));
      }
      public virtual grpc::AsyncUnaryCall<global::Com.Bcom.Solar.Gprc.Empty> SendMessageAsync(global::Com.Bcom.Solar.Gprc.Message request, grpc::CallOptions options)
      {
        return CallInvoker.AsyncUnaryCall(__Method_SendMessage, null, options, request);
      }
      /// <summary>Creates a new instance of client from given <c>ClientBaseConfiguration</c>.</summary>
      protected override SolARMappingAndRelocalizationProxyClient NewInstance(ClientBaseConfiguration configuration)
      {
        return new SolARMappingAndRelocalizationProxyClient(configuration);
      }
    }

    /// <summary>Creates service definition that can be registered with a server</summary>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static grpc::ServerServiceDefinition BindService(SolARMappingAndRelocalizationProxyBase serviceImpl)
    {
      return grpc::ServerServiceDefinition.CreateBuilder()
          .AddMethod(__Method_Init, serviceImpl.Init)
          .AddMethod(__Method_Start, serviceImpl.Start)
          .AddMethod(__Method_Stop, serviceImpl.Stop)
          .AddMethod(__Method_SetCameraParameters, serviceImpl.SetCameraParameters)
          .AddMethod(__Method_RelocalizeAndMap, serviceImpl.RelocalizeAndMap)
          .AddMethod(__Method_Get3DTransform, serviceImpl.Get3DTransform)
          .AddMethod(__Method_Reset, serviceImpl.Reset)
          .AddMethod(__Method_SendMessage, serviceImpl.SendMessage).Build();
    }

    /// <summary>Register service method with a service binder with or without implementation. Useful when customizing the  service binding logic.
    /// Note: this method is part of an experimental API that can change or be removed without any prior notice.</summary>
    /// <param name="serviceBinder">Service methods will be bound by calling <c>AddMethod</c> on this object.</param>
    /// <param name="serviceImpl">An object implementing the server-side handling logic.</param>
    public static void BindService(grpc::ServiceBinderBase serviceBinder, SolARMappingAndRelocalizationProxyBase serviceImpl)
    {
      serviceBinder.AddMethod(__Method_Init, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.PipelineModeValue, global::Com.Bcom.Solar.Gprc.Empty>(serviceImpl.Init));
      serviceBinder.AddMethod(__Method_Start, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty>(serviceImpl.Start));
      serviceBinder.AddMethod(__Method_Stop, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty>(serviceImpl.Stop));
      serviceBinder.AddMethod(__Method_SetCameraParameters, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.CameraParameters, global::Com.Bcom.Solar.Gprc.Empty>(serviceImpl.SetCameraParameters));
      serviceBinder.AddMethod(__Method_RelocalizeAndMap, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.Frame, global::Com.Bcom.Solar.Gprc.RelocalizationResult>(serviceImpl.RelocalizeAndMap));
      serviceBinder.AddMethod(__Method_Get3DTransform, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.RelocalizationResult>(serviceImpl.Get3DTransform));
      serviceBinder.AddMethod(__Method_Reset, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.Empty, global::Com.Bcom.Solar.Gprc.Empty>(serviceImpl.Reset));
      serviceBinder.AddMethod(__Method_SendMessage, serviceImpl == null ? null : new grpc::UnaryServerMethod<global::Com.Bcom.Solar.Gprc.Message, global::Com.Bcom.Solar.Gprc.Empty>(serviceImpl.SendMessage));
    }

  }
}
#endregion
