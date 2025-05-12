# ElasticSearch Setup Guide

We are using `Elasticsearch` docker image as vector storage.

## Setup Guide

### Install ElasticSearch Docker Instance

- First, if not already installed, download `Docker`: https://www.docker.com/products/docker-desktop/

On `Windows`, enable `Linux Containers` Elasticsearch only runs on Linux containers. Follow steps:

- In `Docker Desktop`, right-click the `Docker` icon in the system tray (bottom-right corner).

- If you see `Switch to Linux containers...`, click it.

- If you see `Switch to Windows containers...`, you’re already using Linux containers (good).

### Run Commands

To run **Elasticsearch as a Docker container**, follow these steps:

- Open [`Elasticsearch`](../ElasticSearch/) folder in this repo
- It has [`docker-compose.yml`](docker-compose.yml) file

- Run following command and wait:

```bash
docker compose up -d
```

## Docker Container Tests

Check if `ElasticSearch` docker container is running

```bash
docker ps
```

You should see a container with the image like:

```bash
docker.elastic.co/elasticsearch/elasticsearch:8.12.0
```

Also, open browser with following URL

```bash
http://localhost:9200
```

If `ElasticSearch` is up and running, you will see `json` similar to following:

```json
{
  "name" : "b6e546e91533",
  "cluster_name" : "docker-cluster",
  "cluster_uuid" : "lAXynB3fRm27DhomsVApZg",
  "version" : {
    "number" : "8.12.0",
    "build_flavor" : "default",
    "build_type" : "docker",
    "build_hash" : "1665f706fd9354802c02146c1e6b5c0fbcddfbc9",
    "build_date" : "2024-01-11T10:05:27.953830042Z",
    "build_snapshot" : false,
    "lucene_version" : "9.9.1",
    "minimum_wire_compatibility_version" : "7.17.0",
    "minimum_index_compatibility_version" : "7.0.0"
  },
  "tagline" : "You Know, for Search"
}
```

Vola! you have successfully installed, run and tested `ElasticSearch` on your laptop.

Goto [REAMME.md](../README.md) to progress further.