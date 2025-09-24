<template>
    <FlowComponent
        :stepId="$route.params.stepId" 
        :isEdit="true"
        :id="$route.params.id"
    />
</template>

<script>
    import FlowComponent from '@/components/flow/FlowComponent.vue';
    export default {
        name: "EditFlow",
        components: {
            FlowComponent
        },
    }
</script>