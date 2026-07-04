cmake_minimum_required(VERSION 3.20)
project(EvidenceCppSample CXX)

add_executable(EvidenceCppSample main.cpp)

if (MSVC)
    target_compile_options(EvidenceCppSample PRIVATE /W4 /utf-8)
endif()
