
variable "IMAGE_TAG" {
  default = "latest"
}

target "default" {
  context = "./"
  dockerfile="Dockerfile"
  platforms = [
    "linux/amd64",
    "linux/arm64",
  ]
  tags = [
    "ghcr.io/toras9000/vaultwarden-auto-confirm:${IMAGE_TAG}",
  ]
}
