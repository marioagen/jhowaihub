<template>
    <div class="container mt-2">
        <div class="d-flex flex-nowrap">
            <div class="col-3 kanban-col me-3" v-for="step in kanbanData.steps" :key="step.id">
                <div class="card flex-grow-1">
                    <div class="card-header" :class="findOrder(step.order)">
                        {{step.name}}
                    </div>
                    <div class="card-body" v-for="card in step.cards" :key="card.id">
                        <CardComponent :dataCard="card"
                                       :dataStep="step"
                                       :isFirstStep="firstStep"
                                       label="labelAnalyze"
                                       @reload="reloadList">
                        </CardComponent>
                    </div>
                </div>
            </div>
       </div>
    </div>
</template>
<script>
    import CardComponent from "@/components/global/CardComponent.vue";
    export default {
        name: "WorkflowCards",
        components: {
            CardComponent
        },
        props: {
            kanbanData: {
                type: Array,
                required: false,
                default: () => []
            },
            
        },
        data: () => ({

            firstStep: false,
            customClass: ""
        }),
        methods: {
            findOrder(stepOrder) {
                const minOrder = Math.min(...this.kanbanData.steps.map(s => s.order));
                const maxOrder = Math.max(...this.kanbanData.steps.map(s => s.order));

                if (stepOrder === minOrder) {
                    this.firstStep = true;
                    return `first-steps`;
                }
                else if (stepOrder !== maxOrder) {
                    this.firstStep = false;
                    return `first-steps`;
                    
               }
                else if (stepOrder === maxOrder) {
                    this.firstStep = false;
                    return `last-step`;
               }
            },
            reloadList() {
                this.$emit('reload');
            }
        },
            
    };
</script>
<style scoped>
    .first-steps {
        background-color: #dbe9fc;
    }

    .last-step {
        background-color: #dcfce7;
    }

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-primary {
        background-color: #dbeafe !important;
        color: #2b7fff !important;
    }

    .bg-warning {
        background-color: #fef9c2 !important;
        color: #a65f00 !important;
    }

    .bg-danger {
        background-color: #ffedd4 !important;
        color: #ca3500 !important;
    }

    .bg-success {
        background-color: #d0fae5 !important;
        color: #007a55 !important;
    }

    @media (min-width: 768px) and (max-width: 991.98px) {
        .kanban-col {
            width: 100% !important;
            display: block !important;
        }
    }

</style>
