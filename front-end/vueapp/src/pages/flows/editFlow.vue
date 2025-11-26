<template>
    <FlowComponent
        :isEdit="true"
        :id="routeId"
        :stepId="routeStepId"
        :stepOrder="routeStepOrder"
        :phase="routePhase"
        :workflowId="routeWorkflowId"

    />
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    import { useRoute } from 'vue-router';
    import WorkflowService from "@/services/workflow/WorkflowService";
    export default {
        name: "EditFlow",
        components: {
            FlowComponent
        },
        data() {
            return {
                persistData: {
                },
            };
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
            routeStepId() {
                return parseInt(this.$route.params.stepId);
            },
            routeStepOrder() {
                return parseInt(this.$route.params.stepOrder);
            },
            routePhase() {
                return parseInt(this.$route.params.phase);
            },
            routeWorkflowId() {
                return this.$route.params.workflowId ? parseInt(this.$route.params.workflowId) : null;
            }
        },
    }
</script>