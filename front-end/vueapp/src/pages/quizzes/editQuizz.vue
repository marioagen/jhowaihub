<template>
    <QuizzesForm 
        :isEdit="true"
        :id="routeId"
    />
</template>

<script>
    import QuizzesForm from '@/components/quizzes/QuizzesForm.vue';
    export default {
        name: "EditQuizz",
        components: {
            QuizzesForm
        },
        computed: {
            routeId() {
                return parseInt(this.$route.params.id);
            },
        },
    }
</script>