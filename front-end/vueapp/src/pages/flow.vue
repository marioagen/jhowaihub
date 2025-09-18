<template>
    <main>
        <div class="container-fluid scroll-area mx-2">
            <div class="mt-3 mb-3">
                <div class="card mb-3">
                    <div class="card-body palette">
                        <div
                            class="palette-item"
                            draggable="true"
                            @dragstart="onDragStart($event, {id:0,name:'processo'})"
                        >
                            Processo
                        </div>
                        <div
                            class="palette-item"
                            draggable="true"
                            @dragstart="onDragStart($event,  {id:0,name:'decisao'})"
                        >
                            Decisão
                        </div>
                    </div>
                </div>
                <div class="card mb-3">
                    <div class="card-body vue-flow-container p-0">
                        <VueFlowComponent :stepId="1" :isEditMode="true"/>
                    </div>
                </div>               
            </div>
            <!-- Vueflow stuff -->
        </div>
    </main>
</template>

<script>
    import VueFlowComponent from '@/components/flow/VueFlowComponent.vue';
    export default {
        name: "FlowPage",
        components: {
            VueFlowComponent
        },
        props: {
            stepId: {
                type: Number,
                required: true
            }
        },
    }
</script>
