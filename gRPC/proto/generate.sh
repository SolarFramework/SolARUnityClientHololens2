#!/bin/sh

#
# Config
#
#PROTOC_DIR=$HOME/Applications/gRPC/grpc-protoc_windows_x64-1.39.0-dev
PROTOC_DIR=$GRPC_HOME/bin
PROTO_FILE=sensor_data_sender.proto
#PROTO_FILE=greet.proto

# ====================================================================== #

CPP_OUT_DIR=$PROTO_FILE-gen/cpp
CSHARP_OUT_DIR=$PROTO_FILE-gen/csharp

rm -rf $CPP_OUT_DIR
rm -rf $CSHARP_OUT_DIR

mkdir -p $CPP_OUT_DIR
mkdir -p $CSHARP_OUT_DIR

$PROTOC_DIR/protoc -I ./  --csharp_out=$CSHARP_OUT_DIR $PROTO_FILE --grpc_out=$CSHARP_OUT_DIR --plugin=protoc-gen-grpc=$PROTOC_DIR/grpc_csharp_plugin
$PROTOC_DIR/protoc -I ./  --cpp_out=$CPP_OUT_DIR $PROTO_FILE --grpc_out=$CPP_OUT_DIR --plugin=protoc-gen-grpc=$PROTOC_DIR/grpc_cpp_plugin