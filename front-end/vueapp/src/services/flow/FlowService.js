import api from "@/services/api";

export default {
    getFlowById(flowId) {
        return Promise.resolve(
        {
                nodes:[
                    { 
                        id: '1',
                        position: { x: 250, y: 5 },
                        sourcePosition: "right",
                        label: 'Ínicio',
                        data: { icon: 'CirclePlay', color: 'green', isStartNode: true},
                        type: 'hub',
                    },
                    { 
                        id: '2', 
                        position: { x: 100, y: 100 },
                        label: 'OCR',
                        data: { icon: 'MessageCircle', color: 'blue'},
                        sourcePosition: "right",
                        targetPosition: "left",
                        type: 'hub',                        
                    },
                    { 
                        id: '3', 
                        label: 'Embeddings',
                        position: { x: 400, y: 200 },
                        data: { icon: 'SquareDashed', color: 'orange'},
                        sourcePosition: "right",
                        targetPosition: "left",
                        type: 'hub',                        
                    },
                    {
                        id: '4',
                        position: { x: 500, y: 300 },
                        label: 'Caso de Uso',
                        data: {
                            icon: 'Zap', color: 'purple'
                        },
                        sourcePosition: "right",
                        targetPosition: "left",
                        type: 'hub',
                        
                    },
                ],
                edges:[
  // default bezier edge
  // consists of an edge id, source node id and target node id
  { 
    id: 'e1->2',
    source: '1', 
    target: '2',
        type: 'special',
  },

  // set `animated: true` to create an animated edge path
  { 
    id: 'e2->3',
    source: '2', 
    target: '3', 
    animated: true,
        type: 'special',
  },

  // a custom edge, specified by using a custom type name
  // we choose `type: 'special'` for this example
  {
    animated: true,
    id: 'e3->4',
    type: 'special',
    source: '3',
    target: '4',

    // all edges can have a data object containing any data you want to pass to the edge
    data: {
      edge: 'step',
    }
  },
                    ]
            }); 
        // return api.get(`/Flow/${flowId}`)
        //     .then(({ data }) => {
        //         return data;
        //     })
        //     .catch((error) => {
        //         return {
        //             error: error,
        //         }
        //     });
    },
}