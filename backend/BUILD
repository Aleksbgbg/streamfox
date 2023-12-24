load("@rules_go//go:def.bzl", "go_binary", "go_library")
load("@gazelle//:def.bzl", "gazelle")

gazelle(name = "gazelle")

go_library(
    name = "streamfox-backend_lib",
    srcs = ["main.go"],
    importpath = "streamfox-backend",
    visibility = ["//visibility:private"],
    deps = [
        "//controllers",
        "//files",
        "//models",
        "//utils",
        "@com_github_gin_gonic_gin//:gin",
        "@com_github_joho_godotenv//:godotenv",
    ],
)

filegroup(
    name = "files",
    srcs = [".env"],
)

go_binary(
    name = "streamfox-backend",
    data = ["//:files"],
    embed = [":streamfox-backend_lib"],
    visibility = ["//visibility:public"],
)
