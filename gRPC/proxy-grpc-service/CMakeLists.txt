# Copyright (c) 2021 All Right Reserved, B-com http://www.b-com.com/
#
# This file is subject to the B<>Com License.
# All other rights reserved.
#
# THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
# KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
# IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
# PARTICULAR PURPOSE.

cmake_minimum_required(VERSION 3.5.1)

set(VERBOSE TRUE)
set(CMAKE_VERBOSE_MAKEFILE ON)

project(solar-cloud-proxy C CXX)

# # Change this to your gRPC installation directory
# set(GRPC_HOME "~/.local/grpc/grpc")
#set(GRPC_HOME "~/.local/grpc/gRPC-v1.41.0")

set(gRPC_DIR "${GRPC_HOME}/lib/cmake/grpc")
set(Protobuf_DIR "${GRPC_HOME}/lib/cmake/protobuf")

set(OpenCV_DIR "D:/users/jmhenaff/Applications/opencv/build")

include(grpc.cmake)

find_package( OpenCV REQUIRED )
include_directories( ${OpenCV_INCLUDE_DIRS} )

include_directories("include")
#include_directories("/home/jmhenaff/Applications/grpc/third_party/abseil-cpp")

# Build static lib for gRPC-generated files
add_library(grpc_proto
    "include/sensor_data_sender.grpc.pb.h"
    "src/sensor_data_sender.grpc.pb.cc"
    "include/sensor_data_sender.pb.h"
    "src/sensor_data_sender.pb.cc"
)
target_link_libraries(grpc_proto
  ${_REFLECTION}
  ${_GRPC_GRPCPP}
  ${_PROTOBUF_LIBPROTOBUF})

 # Build executable 
add_executable(solar-cloud-proxy "src/main.cpp")
target_link_libraries(solar-cloud-proxy
    grpc_proto
    ${_REFLECTION}
    ${_GRPC_GRPCPP}
    ${_PROTOBUF_LIBPROTOBUF}
    ${OpenCV_LIBS})