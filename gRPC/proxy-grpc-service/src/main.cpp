/**
 * @copyright Copyright (c) 2021 All Right Reserved, B-com http://www.b-com.com/
 *
 * This file is subject to the B<>Com License.
 * All other rights reserved.
 *
 * THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY
 * KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
 * PARTICULAR PURPOSE.
 *
 */

#include <iostream>
#include <memory>
#include <string>
#include <chrono>
#include <numeric>

#include <grpcpp/ext/proto_server_reflection_plugin.h>
#include <grpcpp/grpcpp.h>
#include <grpcpp/health_check_service_interface.h>

#include <opencv2/opencv.hpp>
#include <opencv2/flann.hpp>

#include "sensor_data_sender.grpc.pb.h"

using grpc::Server;
using grpc::ServerBuilder;
using grpc::ServerContext;
using grpc::Status;
using grpc::StatusCode;
using bcom::solar::cloud::rpc::Message;
using bcom::solar::cloud::rpc::Frame;
using bcom::solar::cloud::rpc::Frames;
using bcom::solar::cloud::rpc::Pose;
using bcom::solar::cloud::rpc::Matrix4x4;
using bcom::solar::cloud::rpc::SolARCloudProxy;
using bcom::solar::cloud::rpc::ImageLayout;

class SolARProxyImpl final : public SolARCloudProxy::Service
{
  typedef std::chrono::system_clock Time;
  typedef std::chrono::milliseconds ms;

  Status Ping(ServerContext* context, const google::protobuf::Empty* request, google::protobuf::Empty* response) override
  {
    std::cout << "Ping received" << std::endl;
    return Status::OK;
  }

  Status SendMessage(ServerContext* context, const Message* request, google::protobuf::Empty* response) override
  {
    std::cout << "Message: " << request->message() << std::endl;
    return Status::OK;
  }

  Status SendFrames(ServerContext* context, const Frames* request, Pose* response) override
  {
    static int iterations = 0;
    auto fps = computeFps();
    if ( iterations % 10 == 0 )
    {
      std::cout << std::to_string(fps) << " FPS" << std::endl;
    }
    iterations = (iterations + 1 ) % 10;
    

    // std::vector<std::shared_ptr<cv::Mat>> images; 

    std::cout << "*******************************************************" << std::endl;
    std::cout << "Frames received: " << std::endl;
    for ( int i = 0; i < request->frames_size(); i++)
    {
      auto f = request->frames().Get(i);

      int image_layout = -1;
      std::string image_layout_string;
      switch(f.image().layout())
      {
        case ImageLayout::RGB_24 : image_layout = CV_8UC4; image_layout_string = "RGB_24";break;
        case ImageLayout::GREY_8 : image_layout = CV_8UC1; image_layout_string = "GREY_8"; break;
        case ImageLayout::GREY_16 : image_layout = CV_8UC2; image_layout_string = "GREY_16"; break;
        default: std::cerr << "Error: unknown image layout" << std::endl; return Status(StatusCode::INVALID_ARGUMENT, "Unknown image layout");
      };

      std::cout << "--------------------------------------------------------" << std::endl;
      std::cout << "  sensor_id: " << f.sensor_id() << std::endl;
      std::cout << "  timestamp: " << f.timestamp() << std::endl;
      std::cout << "  image width: " << f.image().width() << std::endl;
      std::cout << "  image height: " << f.image().height() << std::endl;
      std::cout << "  image layout: " << image_layout_string << std::endl;
      std::cout << "  image data size: " << f.image().data().size() << std::endl;
      std::cout << "--------------------------------------------------------" << std::endl;

      cv::Mat img(
        static_cast<int>(f.image().height()),
        static_cast<int>(f.image().width()),
        image_layout,
        const_cast<void*>(static_cast<const void*>(f.image().data().c_str())));
      drawFps(img);
      cv::imshow("Sensor id: " + std::to_string((int)f.sensor_id()), img);

      // images.push_back(std::make_shared<cv::Mat>(
      //   static_cast<int>(f.image().height()),
      //   static_cast<int>(f.image().width()),
      //   image_layout,
      //   const_cast<void*>(static_cast<const void*>(f.image().data().c_str()))
      // )); 
    } 
    // std::cout << "*******************************************************" << std::endl;

    // int cpt = 0;
    // for (auto& i : images)
    // {
    //   cv::imshow("Sensor id: " + std::to_string(cpt++), *i);
    // } 

    cv::waitKey(1);

    auto mat = response->mutable_mat();
    mat->set_m11(11.f);
    mat->set_m12(12.f);
    mat->set_m13(13.f);
    mat->set_m14(14.f);
    mat->set_m21(21.f);
    mat->set_m22(22.f);
    mat->set_m23(23.f);
    mat->set_m24(24.f);
    mat->set_m31(31.f);
    mat->set_m32(32.f);
    mat->set_m33(33.f);
    mat->set_m34(34.f);
    mat->set_m41(41.f);
    mat->set_m42(42.f);
    mat->set_m44(43.f);
    mat->set_m44(44.f);
    return Status::OK;
  }

  Status SendFrame(ServerContext* context, const Frame* request, Pose* response) override
  {
    std::cout << "*******************************************************" << std::endl;
    std::cout << "Message: " << std::endl;
    std::cout << "--------------------------------------------------------" << std::endl;
    std::cout << "  sensor_id: " << request->sensor_id() << std::endl;
    std::cout << "  timestamp: " << request->timestamp() << std::endl;
    std::cout << "  image width: " << request->image().width() << std::endl;
    std::cout << "  image height: " << request->image().height() << std::endl;
    std::cout << "  image layout: " << ( request->image().layout() == ImageLayout::GREY_8 ? "GREY_8" : "RGB_24") << std::endl;
    std::cout << "--------------------------------------------------------" << std::endl;
  
    int image_layout = request->image().layout() == ImageLayout::GREY_8 ? CV_8UC1 : CV_8UC4;

    cv::Mat img(
      static_cast<int>(request->image().height()),
      static_cast<int>(request->image().width()),
      image_layout,
      const_cast<void*>(static_cast<const void*>(request->image().data().c_str())));

    drawFps(img);

    cv::imshow("Sensor id: " + std::to_string((int)request->sensor_id()), img);
    
    std::cout << "*******************************************************" << std::endl;


    cv::waitKey(1);

    auto mat = response->mutable_mat();
    mat->set_m11(11.f);
    mat->set_m12(12.f);
    mat->set_m13(13.f);
    mat->set_m14(14.f);
    mat->set_m21(21.f);
    mat->set_m22(22.f);
    mat->set_m23(23.f);
    mat->set_m24(24.f);
    mat->set_m31(31.f);
    mat->set_m32(32.f);
    mat->set_m33(33.f);
    mat->set_m34(34.f);
    mat->set_m41(41.f);
    mat->set_m42(42.f);
    mat->set_m44(43.f);
    mat->set_m44(44.f);
    return Status::OK;
  }

  private:

  float computeFps()
  {
    auto now = Time::now();
    auto timeElapsed = now - last_time;
    last_time = now;
    last_ten_deltas.push_back(std::chrono::duration_cast<ms>(timeElapsed).count());
    if (last_ten_deltas.size() > 10)
    {
      last_ten_deltas.erase(last_ten_deltas.begin());
    }
    float average = std::accumulate(last_ten_deltas.begin(), last_ten_deltas.end(), 0.f) / last_ten_deltas.size();

    return 1000.f / average;
  } 

  void drawFps(cv::Mat img)
  {
    cv::putText(img,
                (std::string("FPS:") + std::to_string(computeFps())).c_str(),
                cv::Point(10, 40), //top-left position
                cv::FONT_HERSHEY_DUPLEX,
                1.0,
                CV_RGB(118, 185, 0),
                2);
  }

  std::vector<float> last_ten_deltas;
  std::chrono::time_point<std::chrono::system_clock> last_time;
};


void RunServer() {
  std::string server_address("0.0.0.0:5002");
  //GreeterServiceImpl service;
  SolARProxyImpl service;

  grpc::EnableDefaultHealthCheckService(true);
  grpc::reflection::InitProtoReflectionServerBuilderPlugin();
  ServerBuilder builder;
  // Listen on the given address without any authentication mechanism.
  builder.AddListeningPort(server_address, grpc::InsecureServerCredentials());
  // Register "service" as the instance through which we'll communicate with
  // clients. In this case it corresponds to an *synchronous* service.
  builder.RegisterService(&service);
  // Finally assemble the server.
  std::unique_ptr<Server> server(builder.BuildAndStart());
  std::cout << "Server listening on " << server_address << std::endl;

  // Wait for the server to shutdown. Note that some other thread must be
  // responsible for shutting down the server for this call to ever return.
  server->Wait();
}

int main(int argc, char** argv) {
  RunServer();

  return 0;
}
