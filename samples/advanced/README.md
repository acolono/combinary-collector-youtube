# Advanced example
Using [docker-compose](https://docs.docker.com/compose/) to simulate the infrastructure.  
This example uses elasticsearch, so you may have to [adjust your `vm.max_map_count`](https://www.elastic.co/guide/en/elasticsearch/reference/current/docker.html).

API_KEYS are required
```sh
export API_KEYS="<api-keys>"
```
Channel or list ID's are optional
```sh
export CHANNEL_IDS="<channel-ids>"
export LIST_IDS="<list-ids>"
```
Look at the [docker-compose.yml](./docker-compose.yml) file for other options.

Start the whole stack with:
```
docker-compose up -d
```

Once the data is availabe you can explore it in kibana on [localhost:5601](http://localhost:5601).