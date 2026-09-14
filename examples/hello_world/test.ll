; ModuleID = 'test'
source_filename = "test"

@"$test_V_N8<global>1a" = global i32 1
@"$test_V_N8<global>1b" = global i32 0
@llvm.global_ctors = appending global [1 x { i32, ptr, ptr }] [{ i32, ptr, ptr } { i32 65535, ptr @"test_<GlobalInitializer>", ptr null }]

define i32 @"$test_F_N8<global>3addE_I32_I32_R_I32"(i32 %0, i32 %1) {
entry:
  %2 = add i32 %0, %1
  ret i32 %2
}

define double @"$test_F_N8<global>3addE_F32_I32_R_F64"(float %0, i32 %1) {
entry:
  %2 = fpext float %0 to double
  %3 = sitofp i32 %1 to double
  %4 = fadd double %2, %3
  ret double %4
}

define i32 @"$test_F_N8<global>3maxE_I32_I32_R_I32"(i32 %0, i32 %1) {
entry:
  %2 = icmp sgt i32 %0, %1
  br i1 %2, label %cond.then, label %cond.else

cond.then:                                        ; preds = %entry
  br label %cond.merge

cond.else:                                        ; preds = %entry
  br label %cond.merge

cond.merge:                                       ; preds = %cond.else, %cond.then
  %3 = phi i32 [ %0, %cond.then ], [ %1, %cond.else ]
  ret i32 %3
}

define i32 @"$test_F_N8<global>7if_trueE_B_I32_I32_R_I32"(i8 %0, i32 %1, i32 %2) {
entry:
  %3 = icmp ne i8 %0, 0
  br i1 %3, label %cond.then, label %cond.else

cond.then:                                        ; preds = %entry
  br label %cond.merge

cond.else:                                        ; preds = %entry
  br label %cond.merge

cond.merge:                                       ; preds = %cond.else, %cond.then
  %4 = phi i32 [ %1, %cond.then ], [ %2, %cond.else ]
  ret i32 %4
}

define i32 @"$test_F_N8<global>10cast_checkE_I64_R_I32"(i64 %0) {
entry:
  %1 = trunc i64 %0 to i32
  ret i32 %1
}

define i32 @"$test_F_N8<global>3minE_I32_I32_R_I32"(i32 %0, i32 %1) {
entry:
  %result = alloca i32, align 4
  %2 = icmp slt i32 %0, %1
  br i1 %2, label %cond.then, label %cond.else

cond.then:                                        ; preds = %entry
  store i32 %0, ptr %result, align 4
  br label %cond.merge

cond.else:                                        ; preds = %entry
  store i32 %1, ptr %result, align 4
  br label %cond.merge

cond.merge:                                       ; preds = %cond.else, %cond.then
  %3 = load i32, ptr %result, align 4
  ret i32 %3
}

define i32 @"$test_F_N8<global>15compound_assignE_I32_I32_R_I32"(i32 %0, i32 %1) {
entry:
  %a = alloca i32, align 4
  store i32 %0, ptr %a, align 4
  %2 = load i32, ptr %a, align 4
  %3 = add i32 %2, %1
  store i32 %3, ptr %a, align 4
  %4 = load i32, ptr %a, align 4
  ret i32 %4
}

define i32 @"$test_F_N8<global>4mainE_R_I32"() {
entry:
  %0 = call i32 @"$test_F_N8<global>3addE_I32_I32_R_I32"(i32 1, i32 4)
  ret i32 %0
}

define internal void @"test_<GlobalInitializer>"() {
entry:
  %0 = load i32, ptr @"$test_V_N8<global>1a", align 4
  %1 = add i32 %0, 2
  store i32 %1, ptr @"$test_V_N8<global>1b", align 4
  ret void
}

define i32 @main() {
entry:
  %0 = call i32 @"$test_F_N8<global>4mainE_R_I32"()
  ret i32 %0
}
