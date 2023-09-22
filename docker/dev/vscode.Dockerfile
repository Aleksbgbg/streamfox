FROM gitpod/openvscode-server:latest

ENV OPENVSCODE="/home/.openvscode-server/bin/openvscode-server"

SHELL ["/bin/bash", "-c"]
RUN \
  exts=( \
  marlosirapuan.nord-deep \
  emeraldwalk.runonsave \
  golang.go \
  vue.volar \
  johnsoncodehk.vscode-typescript-vue-plugin \
  bradlc.vscode-tailwindcss \
  esbenp.prettier-vscode \
  dbaeumer.vscode-eslint \
  orta.vscode-jest \
  ms-azuretools.vscode-docker \
  ) \
  # Install the $exts
  && for ext in "${exts[@]}"; do ${OPENVSCODE} --install-extension "${ext}"; done
